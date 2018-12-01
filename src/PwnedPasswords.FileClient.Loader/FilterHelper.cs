using McMaster.Extensions.CommandLineUtils;
using PwnedPasswords.BloomFilter;
using System;
using System.IO;

namespace PwnedPasswords.FileClient.Loader
{
    internal class FilterHelper
    {
        private const int ReportPasswordsEveryN = 100_000;
        private const char Sha1PrevalenceSeparator = ':';

        public static int CountLinesInPwnedPasswordsFile(string filename, int minimumPrevalence, IConsole console)
        {
            // Count the number of lines in the file that have the required prevalence
            var capacity = 0;
            console.WriteLine($"Counting lines in PwnedPasswords file '{filename}'");
            foreach (var line in File.ReadLines(filename))
            {
                var span = line.AsSpan();
                var spaceIndex = span.LastIndexOf(Sha1PrevalenceSeparator);
                if (spaceIndex < 0)
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - should have format <SHA1HASH prevalence>. Found <{line}>");
                }

                var numberAsSpan = span.Slice(spaceIndex + 1);
                if (!int.TryParse(numberAsSpan, out var prevalence))
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - Prevalance <{numberAsSpan.ToString()}> was not a valid integer: Line <{line}>");
                }

                if (prevalence < minimumPrevalence)
                {
                    // file is ordered, so we can bail out
                    console.WriteLine($"Found password with prevalence <{prevalence}>, less than minimum <{minimumPrevalence}>. Found {capacity} passwords.");
                    return capacity;
                }
                capacity++;
            }

            console.WriteLine($"Whole file read. Found {capacity} passwords.");
            return capacity;
        }

        public static int LoadPwnedPasswordsFileIntoFilter(string pwnedPasswordsFile, BloomFilter.BloomFilter filter, int minimumPrevalence, IConsole console)
        {
            var passwordCount = 0;
            console.WriteLine($"Reading passwords in PwnedPasswords file {pwnedPasswordsFile}");
            foreach (var line in File.ReadLines(pwnedPasswordsFile))
            {
                var span = line.AsSpan();
                var spaceIndex = span.LastIndexOf(Sha1PrevalenceSeparator);
                if (spaceIndex < 0)
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - should have format <SHA1HASH prevalence>. Found <{line}>");
                }
                var numberAsSpan = span.Slice(spaceIndex + 1);
                if (!int.TryParse(numberAsSpan, out var prevalence))
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - Prevalance <{numberAsSpan.ToString()}> was not a valid integer: Line <{line}>");
                }

                var sha = span.Slice(0, spaceIndex).ToString();
                filter.Add(sha);

                if (prevalence < minimumPrevalence)
                {
                    // file is ordered, so we can bail out now
                    console.WriteLine($"Found password with prevalence <{prevalence}>, less than minimum <{minimumPrevalence}>. Loaded {passwordCount} passwords.");
                    return passwordCount;
                }
                passwordCount++;

                if (passwordCount % ReportPasswordsEveryN == 0)
                {
                    console.WriteLine($"{passwordCount} loaded...");
                }
            }

            console.WriteLine($"Whole file read. Found {passwordCount} passwords.");
            return passwordCount;
        }
    }
}