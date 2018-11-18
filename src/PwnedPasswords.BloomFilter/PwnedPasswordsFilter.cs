using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace PwnedPasswords.BloomFilter
{
    public class PwnedPasswordsClientFactory
    {
        private readonly ILogger<PwnedPasswordsClientFactory> _logger;

        public PwnedPasswordsClientFactory(ILogger<PwnedPasswordsClientFactory> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Builds a <see cref="PwnedPasswordsClient"/> by loading the provided files
        /// into a bloom filter with the provided expected error rate. The file should use the 
        /// Version 3 format as defined in https://haveibeenpwned.com/Passwords - SHA1 passwords
        /// ordered by prevalence.
        /// </summary>
        /// <param name="filename">The file containing the SHA1 pwned passwords</param>
        /// <param name="expectedErrorRate">The expected false positive rate of the generated filter</param>
        /// <param name="minimumPrevalence">Only load files with at least this prevalance. A prevalance of 1 means all passwords</param>
        /// <returns></returns>
        public PwnedPasswordsClient CreateFromPwnedPasswordsFile(string filename, float expectedErrorRate, int minimumPrevalence = 1)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentException("message", nameof(filename));
            if (minimumPrevalence < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumPrevalence), minimumPrevalence,
                     $"{nameof(minimumPrevalence)} must be at least 1. Was {minimumPrevalence}");
            }
            if (expectedErrorRate >= 1 || expectedErrorRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedErrorRate), expectedErrorRate,
                    $"{nameof(expectedErrorRate)} must be between 0 and 1, exclusive. Was {expectedErrorRate}");
            }

            var capacity = CountLines(filename, minimumPrevalence);

            var filter = new BloomFilter(capacity, expectedErrorRate);

            var passwordCount = LoadFile(filename, filter, minimumPrevalence);

            return new PwnedPasswordsClient(filter);
        }

        /// <summary>
        /// Builds a <see cref="PwnedPasswordsClient"/> by loading the provided bloom filter file
        /// </summary>
        /// <param name="filename">The file containing the saved bloom filter list</param>
        /// <returns></returns>
        public PwnedPasswordsClient CreateFromSavedFilter(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentException("message", nameof(filename));
            var filter = BloomFilter.Load(filename);
            return new PwnedPasswordsClient(filter);
        }

        private static int CountLines(string filename, int minimumPrevalence)
        {
            // Count the number of lines in the file that have the required prevalence
            var capacity = 0;
            foreach (var line in File.ReadLines(filename))
            {
                var span = line.AsSpan();
                var spaceIndex = span.LastIndexOf(' ');
                if (spaceIndex < 0)
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - should have format <SHA1HASH prevalence>. Found <{line}>");
                }
                var numberAsString = span.Slice(spaceIndex + 1).ToString();
                if (!int.TryParse(numberAsString, out var prevalence))
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - Prevalance <{numberAsString}> was not a valid integer: Line <{line}>");
                }

                if (prevalence < minimumPrevalence)
                {
                    // file is ordered, so we can bail out
                    return capacity;
                }
                capacity++;
            }

            return capacity;
        }

        private static int LoadFile(string pwnedPasswordsFile, BloomFilter filter, int minimumPrevalence)
        {
            var passwordCount = 0;
            foreach (var line in File.ReadLines(pwnedPasswordsFile))
            {
                var span = line.AsSpan();
                var spaceIndex = span.LastIndexOf(' ');
                if (spaceIndex < 0)
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - should have format <SHA1HASH prevalence>. Found <{line}>");
                }
                var numberAsString = span.Slice(spaceIndex + 1).ToString();
                if (!int.TryParse(numberAsString, out var prevalence))
                {
                    throw new InvalidFileFormatException($@"The file didn't have the expected format - Prevalance <{numberAsString}> was not a valid integer: Line <{line}>");
                }

                var sha = span.Slice(0, spaceIndex).ToString();
                filter.Add(sha);

                if (prevalence < minimumPrevalence)
                {
                    // file is ordered, so we can bail out now
                    return passwordCount;
                }
                passwordCount++;
            }

            return passwordCount;
        }
    }
}
