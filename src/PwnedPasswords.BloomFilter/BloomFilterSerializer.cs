using System;
using System.Collections;
using System.IO;

namespace PwnedPasswords.BloomFilter
{
    partial class BloomFilter
    {
        /// <summary>
        /// Loads a <see cref="BloomFilter"/> from a byte array
        /// The filter must have been created using <see cref="Save(BloomFilter)"/>
        /// </summary>
        public static BloomFilter Load(byte[] bytes)
        {
            return BloomFilterSerializer.Load(bytes);
        }

        /// <summary>
        /// Loads a <see cref="BloomFilter"/> from a file
        /// The filter must have been created using <see cref="Save(BloomFilter)"/>
        /// </summary>
        public static BloomFilter Load(string filePath)
        {
            return Load(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Save the bloom filter to a byte array, which can later be loaded using <see cref="Load(byte[])"/>
        /// </summary>
        /// <param name="filter"></param>
        public static byte[] Save(BloomFilter filter)
        {
            return BloomFilterSerializer.Save(filter);
        }

        /// <summary>
        /// Saves the bloom filter to a file, which can later be loaded using <see cref="Load(byte[])"/>
        /// </summary>
        public static void Save(BloomFilter filter, string filePath)
        {
            File.WriteAllBytes(filePath, Save(filter));
        }

        private class BloomFilterSerializer
        {
            private const int HeaderBytes =
                4 // HashFunctionCount
                + 4 // HashBits.Count
                + 4 // Capacity
                + 4; // ErrorRate

            public static BloomFilter Load(byte[] bytes)
            {
                if (bytes == null) throw new ArgumentNullException(nameof(bytes));
                if (bytes.Length < 7) throw new ArgumentException("The provided bytes are not a valid bloom filter");

                //read the number of hash functions
                Span<byte> span = bytes;
                var hashFunctionCount = span.Slice(0, 4).GetAsInt();
                var bitArrayCapacity = span.Slice(4, 4).GetAsInt();
                var filterCapacity = span.Slice(8, 4).GetAsInt();
                var filterErrorRate = span.Slice(12, 4).GetAsFloat();
                AssertExpectedLength(bytes, bitArrayCapacity);

                var slicedBytes = span.Slice(HeaderBytes).ToArray();
                var bitArray = new BitArray(slicedBytes);
                bitArray.Length = bitArrayCapacity;

                return new BloomFilter(bitArray, hashFunctionCount, filterCapacity, filterErrorRate);
            }

            public static byte[] Save(BloomFilter filter)
            {
                int filterBytes = filter.HashBits.Count / 8;
                if (filterBytes * 8 != filter.HashBits.Count)
                {
                    // partially filled final byte
                    filterBytes++;
                }
                var arrayLength = filterBytes + HeaderBytes;
                var data = new byte[arrayLength];

                // save the headers
                filter.HashFunctionCount.GetAsBytes().CopyTo(data, 0);
                filter.HashBits.Count.GetAsBytes().CopyTo(data, 4);
                filter.Capacity.GetAsBytes().CopyTo(data, 8);
                filter.ExpectedErrorRate.GetAsBytes().CopyTo(data, 12);

                // now copy the actual data
                filter.HashBits.CopyTo(data, 16);

                return data;
            }

            static void AssertExpectedLength(byte[] bytes, int capacity)
            {
                int expectedBytes = capacity / 8;
                if (expectedBytes * 8 != capacity)
                {
                    // partially filled final byte
                    expectedBytes++;
                }
                expectedBytes += HeaderBytes; // + HeaderBytes for capacity values at start of array

                if (expectedBytes != bytes.Length)
                {
                    throw new ArgumentException($"Provided bytes had wrong length: expected {expectedBytes} but found {bytes.Length}");
                }
            }
        }
    }
}
