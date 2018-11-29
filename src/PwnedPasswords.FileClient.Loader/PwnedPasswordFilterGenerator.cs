using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PwnedPasswords.FileClient.Loader
{
    [Command(
          Name = "pwned passwords filter generator",
          FullName = "pwned-passwords-filter-generator",
          Description = "Load a file of pwned passwords and generate a bloom filter",
          ExtendedHelpText = Constants.ExtendedHelpText)]
    [HelpOption]
    internal class PwnedPasswordFilterGenerator
    {
        [Required(ErrorMessage = "You must specify the path to where to save the generated filter")]
        [Argument(0, Name = "output", Description = "Path to where the output filter should be written")]
        public string Output { get; }

        [Required(ErrorMessage = "You must specify at least one source file")]
        [Argument(1, Name = "source", Description = "Path to the output files to use to generate the filter")]
        [FileOrDirectoryExists]
        public string[] Source { get; }

        [Option("-e|--expected-error-rate", CommandOptionType.SingleValue, Description = "The expected error rate to use when generating the filter. Defaults to 0.1%")]
        [Range(0, 1)]
        public float ExpectedErrorRate { get; } = 0.001f;

        [Option("-p|--minimum-prevalence", CommandOptionType.SingleValue, Description = "The minimum prevalance of passwords to include. Passwords of less than this value will be ignored. Includes all values by default")]
        [Range(1, int.MaxValue)]
        public int MinimumPrevalence { get; } = 1;

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            // 517238891
            var sw = new Stopwatch();

            var capacities = new int[Source.Length];
            for (int i = 0; i < Source.Length; i++)
            {
                capacities[i] = FilterHelper.CountLinesInPwnedPasswordsFile(Source[i], MinimumPrevalence, console);
            }

            var capacity = capacities.Sum();

            console.WriteLine($"Total required capacity: {capacity}");
            console.WriteLine($"Creating filter with capacity <{capacity}>, expected error rate <{ExpectedErrorRate}>");

            var filter = new BloomFilter.BloomFilter(capacity, ExpectedErrorRate);

            for (int i = 0; i < Source.Length; i++)
            {
                var passwordsRead = FilterHelper.LoadPwnedPasswordsFileIntoFilter(Source[i], filter, MinimumPrevalence, console);
                if(passwordsRead != capacities[i])
                {
                    throw new Exception($"Error reading passwords - number of passwords read <{passwordsRead}> did not match number expected <{capacities[i]}>");
                }
            }

            console.WriteLine($"Filter built successfully, writing to filter output file: {Output}");

            filter.Save(Output);

            var duration = sw.Elapsed;
            console.WriteLine($"Filter saved OK - duration: {duration.ToString()}");

            return Program.OK;
        }
    }
}