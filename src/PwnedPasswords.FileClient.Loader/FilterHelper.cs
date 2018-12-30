using McMaster.Extensions.CommandLineUtils;
using PwnedPasswords.BloomFilter;
using System;
using System.IO;

namespace PwnedPasswords.FileClient.Loader
{
    internal class FilterHelper
    {
        private const int ReportPasswordsEveryN = 1_000_000;
        private const char Sha1PrevalenceSeparator = ':';

        public static long CountLinesInPwnedPasswordsFile(string filename, int minimumPrevalence, IConsole console)
        {
            // Count the number of lines in the file that have the required prevalence
            long capacity = 0;
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

        public static long LoadPwnedPasswordsFileIntoFilter(string pwnedPasswordsFile, BloomFilter.BloomFilter filter, int minimumPrevalence, IConsole console)
        {
            long passwordCount = 0;
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
                    console.Write($"{passwordCount:n0} loaded...");
                    console.Write("\r"); // go back to start of line so we overwrite this later
                }
            }

            console.WriteLine($"Whole file read. Found {passwordCount} passwords.");
            return passwordCount;
        }

        public static VerificationResult CheckPasswordExistenceInFilter(
            string pwnedPasswordsFile, BloomFilter.BloomFilter filter, int minimumPrevalence, IConsole console)
        {
            long hitsAbove = 0;
            long hitsBelow = 0;
            long missesAbove = 0;
            long missesBelow = 0;
            long passwordCount = 0;

            console.WriteLine($"Checking passwords in PwnedPasswords file {pwnedPasswordsFile}");
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
                var isHit = filter.Contains(sha);

                if (prevalence < minimumPrevalence)
                {
                    if (isHit) { hitsBelow++; }
                    else { missesBelow++; }
                }
                else
                {
                    if (isHit) { hitsAbove++; }
                    else { missesAbove++; }
                }

                passwordCount++;
                if (passwordCount % ReportPasswordsEveryN == 0)
                {
                    console.Write($"{passwordCount:n0} checked...");
                    console.Write("\r"); // go back to start of line so we overwrite this later
                }
            }

            var verification = new VerificationResult(hitsAbove, hitsBelow, missesAbove, missesBelow);
            console.WriteLine($"Verification complete. {verification}");

            return verification;
        }

    }
}