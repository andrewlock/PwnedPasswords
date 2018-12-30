using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace PwnedPasswords.FileClient.Loader
{
    [Command(
          Name = "pwned-passwords-tool",
          FullName = "Pwned Passwords bloom filter tool",
          Description = "A tool for working with pwned passwords to generate and verify bloom filters"),
     Subcommand(nameof(Create), typeof(Create)),
     Subcommand(nameof(Verify), typeof(Verify)),
     Subcommand(nameof(Info), typeof(Info)),
    ]
    [HelpOption]
    internal class PwnedPasswordBloomFilterTool
    {
        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify a subcommand.");
            app.ShowHelp();
            return 1;
        }

        [Command("create", Description = "Create a bloom filter file from pwned password files"), HelpOption]
        class Create
        {
            [Required(ErrorMessage = "You must specify the path to where to save the generated filter")]
            [Argument(0, Name = "output", Description = "Path to the filter file")]
            public string FilterFile { get; }

            [Required(ErrorMessage = "You must specify at least one source file")]
            [Argument(1, Name = "source", Description = "Path to the pwned password files to use to generate the filter. Must be sorted by password prevalence")]
            [FileOrDirectoryExists]
            public string[] Sources { get; }

            [Option("-e|--expected-error-rate", CommandOptionType.SingleValue, Description = "The expected error rate to use when generating the filter. Defaults to 0.1%")]
            [Range(0f, 1f)]
            public float ExpectedErrorRate { get; } = 0.001f;

            [Option("-p|--minimum-prevalence", CommandOptionType.SingleValue, Description = "The minimum prevalence of passwords to include. Passwords of less than this value will be ignored. Includes all values by default")]
            [Range(1, int.MaxValue)]
            public int MinimumPrevalence { get; } = 1;

            [Option("-n|--number-of-passwords", CommandOptionType.SingleValue, Description = "The number of passwords in the file that match the minimum prevalence. Avoids reading file twice when number of passwords is known. Can only be used if you have a single source file")]
            [Range(1, long.MaxValue)]
            public long? NumberOfPasswords { get; }

            [Option("-v|--verbose", CommandOptionType.NoValue, Description = "If provided, after generating the file all passwords are tested to ensure no false negatives, and that the false positive error rate has not been exceeded.")]
            public bool Validate { get; }

            public ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
            {
                if (NumberOfPasswords.HasValue && Sources.Length != 1)
                {
                    return new ValidationResult("You can only specify --number-of-passwords if you provide a single file source");
                }

                return ValidationResult.Success;
            }

            public int OnExecute(CommandLineApplication app, IConsole console)
            {
                var sw = new Stopwatch();
                sw.Start();

                var capacities = new long[Sources.Length];
                if (NumberOfPasswords.HasValue)
                {
                    Debug.Assert(Sources.Length == 1);
                    capacities[0] = NumberOfPasswords.Value;
                }
                else
                {
                    for (int i = 0; i < Sources.Length; i++)
                    {
                        capacities[i] = FilterHelper.CountLinesInPwnedPasswordsFile(Sources[i], MinimumPrevalence, console);
                    }
                }

                var capacity = capacities.Sum();

                console.WriteLine($"Total required capacity: {capacity}");
                console.WriteLine($"Creating filter with capacity <{capacity}>, expected error rate <{ExpectedErrorRate}>");

                var filter = new BloomFilter.BloomFilter(capacity, ExpectedErrorRate);

                for (int i = 0; i < Sources.Length; i++)
                {
                    var passwordsRead = FilterHelper.LoadPwnedPasswordsFileIntoFilter(Sources[i], filter, MinimumPrevalence, console);
                    if (passwordsRead != capacities[i])
                    {
                        throw new Exception($"Error reading passwords - number of passwords read <{passwordsRead}> did not match number expected <{capacities[i]}>");
                    }
                }

                console.WriteLine($"Filter built successfully, writing to filter output file: {FilterFile}");

                filter.Save(FilterFile);

                var duration = sw.Elapsed;
                console.WriteLine($"Filter saved OK - duration: {duration.ToString("g")}");

                if (Validate)
                {
                    var isValid = Verify.IsValid(console, filter, Sources, ExpectedErrorRate, MinimumPrevalence);
                    if (!isValid)
                    {
                        return Program.ERROR;
                    }
                }
                
                return Program.OK;
            }
        }

        [Command("verify", Description = "Verifies a bloom filter file contains the expected data"), HelpOption]
        class Verify
        {
            [Required(ErrorMessage = "You must specify the path from where to load the bloom filter")]
            [Argument(0, Name = "output", Description = "Path to the filter file")]
            [FileOrDirectoryExists]
            public string FilterPath { get; }

            [Required(ErrorMessage = "You must specify at least one password file")]
            [Argument(1, Name = "source", Description = "Path to the pwned password files usef to generate the filter. Must be sorted by password prevalence")]
            [FileOrDirectoryExists]
            public string[] Sources { get; }

            [Option("-e|--expected-error-rate", CommandOptionType.SingleValue, Description = "The expected error rate used when generating the filter. this is the expected error rate for passwords below the minimum threshold. Defaults to 0.1%")]
            [Range(0f, 1f)]
            public float ExpectedErrorRate { get; } = 0.001f;

            [Option("-p|--minimum-prevalence", CommandOptionType.SingleValue, Description = "The minimum prevalence of passwords included. All passwords of this threshold and above should be found by the filter. Passwords below this threshold will return hisses approximately equaling the expected error rate Includes all values by default")]
            [Range(1, int.MaxValue)]
            public int MinimumPrevalence { get; } = 1;

            public int OnExecute(CommandLineApplication app, IConsole console)
            {
                BloomFilter.BloomFilter filter;
                try
                {
                    console.WriteLine($"Loading filter from '{FilterPath}'...");
                    filter = BloomFilter.BloomFilter.Load(FilterPath);
                }
                catch (Exception)
                {
                    console.WriteLine($"Error loading filter from '{FilterPath}'");
                    throw;
                }

                console.WriteLine("Filter loaded successfully. Verifying passwords...");

                var isValid = IsValid(console, filter, Sources, ExpectedErrorRate, MinimumPrevalence);

                return isValid ? Program.OK : Program.ERROR;
            }

            public static bool IsValid(IConsole console, BloomFilter.BloomFilter filter, string[] sources, float expectedErrorRate, int minimumPrevalence)
            {
                var total = VerificationResult.Empty;
                foreach (var source in sources)
                {
                    var verification = FilterHelper.CheckPasswordExistenceInFilter(source, filter, minimumPrevalence, console);
                    total = total.Add(verification);
                }

                var isValid = true;
                if (total.MissesAboveThreshold != 0)
                {
                    isValid = false;
                    console.Error.WriteLine(
                        $"ERROR: The filter did not contain all the expected passwords. {total.MissesAboveThreshold}/{total.PasswordsAboveThreshold} failed");
                }

                var expectedHits = (long) (total.PasswordsBelowThreshold * expectedErrorRate);
                var actualErrorRate = ((double) total.HitsBelowThreshold) / total.PasswordsBelowThreshold;
                if (total.HitsBelowThreshold > (expectedHits * 2)) // build in some leeway
                {
                    isValid = false;
                    console.Error.WriteLine(
                        $"ERROR: The filter generated an unexpected number of false positives. For {total.PasswordsBelowThreshold} passwords below threshold, with expected error rate of {expectedErrorRate}, expected at most {expectedHits} false positives. Actually had {total.HitsBelowThreshold} false positives ({actualErrorRate} error rate)");
                }
                else if(total.PasswordsBelowThreshold > 0)
                {
                    console.WriteLine($"Achieved error rate of {RoundToSignificantDigits(actualErrorRate, 2)}");
                }

                return isValid;
            }

            static double RoundToSignificantDigits(double d, int digits)
            {
                if (d == 0) { return 0;}

                var scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
                return scale * Math.Round(d / scale, digits);
            }

        }

        [Command("info", Description = "Prints information about a bloom filter file"), HelpOption]
        class Info
        {
            [Required(ErrorMessage = "You must specify the path from where to load the bloom filter")]
            [Argument(0, Name = "output", Description = "Path to the filter file")]
            [FileOrDirectoryExists]
            public string FilterPath { get; }

            public int OnExecute(CommandLineApplication app, IConsole console)
            {
                BloomFilter.BloomFilter filter;
                try
                {
                    console.WriteLine($"Loading filter from '{FilterPath}'...");
                    filter = BloomFilter.BloomFilter.Load(FilterPath);
                }
                catch (Exception)
                {
                    console.WriteLine($"Error loading filter from '{FilterPath}'");
                    throw;
                }

                console.WriteLine("Filter loaded successfully.");
                console.WriteLine(filter.PrettyPrint());

                return Program.OK;
            }
        }
    }
}