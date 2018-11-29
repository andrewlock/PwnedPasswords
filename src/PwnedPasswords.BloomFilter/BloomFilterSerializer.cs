using System;
using System.Collections;
using System.IO;

namespace PwnedPasswords.BloomFilter
{
    partial class BloomFilter
    {
        /// <summary>
        /// Loads a <see cref="BloomFilter"/> from a byte array
        /// The filter must have been created using <see cref="Save()"/>
        /// </summary>
        public static BloomFilter Load(byte[] bytes)
        {
            return BloomFilterSerializer.Load(bytes);
        }

        /// <summary>
        /// Loads a <see cref="BloomFilter"/> from a file
        /// The filter must have been created using <see cref="Save()"/>
        /// </summary>
        public static BloomFilter Load(string filePath)
        {
            return Load(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Save the bloom filter to a byte array, which can later be loaded using <see cref="Load(byte[])"/>
        /// </summary>
        public byte[] Save()
        {
            return BloomFilterSerializer.Save(this);
        }

        /// <summary>
        /// Saves the bloom filter to a file, which can later be loaded using <see cref="Load(byte[])"/>
        /// </summary>
        public void Save(string filePath)
        {
            File.WriteAllBytes(filePath, Save());
        }

        private class BloomFilterSerializer
        {
            private const int HeaderBytes =
                4 // HashFunctionCount
                + 4 // HashBits.Count (shards)
                + 4 // Shard Capacity
                + 4; // ErrorRate

            public static BloomFilter Load(byte[] bytes)
            {
                if (bytes == null) throw new ArgumentNullException(nameof(bytes));
                if (bytes.Length < 7) throw new ArgumentException("The provided bytes are not a valid bloom filter");

                //read the number of hash functions
                Span<byte> span = bytes;
                var hashFunctionCount = span.Slice(0, 4).GetAsInt();
                var shardCount = span.Slice(4, 4).GetAsInt();
                var shardCapacity = span.Slice(8, 4).GetAsInt();
                var filterErrorRate = span.Slice(12, 4).GetAsFloat();
                AssertExpectedLength(bytes, shardCount, shardCapacity);

                var index = HeaderBytes;
                var bitArrays = new BitArray[shardCount];
                var shardBytes = GetBytesCount(shardCapacity);

                for (int i = 0; i < shardCount; i++)
                {
                    var slicedBytes = span.Slice(index, shardBytes).ToArray();
                    bitArrays[i] = new BitArray(slicedBytes);
                    bitArrays[i].Length = shardCapacity;
                }

                return new BloomFilter(bitArrays, hashFunctionCount, shardCapacity, filterErrorRate);
            }

            public static byte[] Save(BloomFilter filter)
            {
                var shardBytes = GetBytesCount(filter.ShardCapacity);
                var arrayLength = (long)shardBytes*filter.Shards + HeaderBytes;
                var data = new byte[arrayLength];

                // save the headers
                filter.HashFunctionCount.GetAsBytes().CopyTo(data, 0);
                filter.Shards.GetAsBytes().CopyTo(data, 4);
                filter.ShardCapacity.GetAsBytes().CopyTo(data, 8);
                filter.ExpectedErrorRate.GetAsBytes().CopyTo(data, 12);

                // now copy the actual data
                var index = HeaderBytes;
                foreach (var hashBits in filter.HashBits)
                {
                    hashBits.CopyTo(data, index);
                    index += shardBytes;
                }

                return data;
            }

            static int GetBytesCount(int shardCapacity)
            {
                var shardBytes = shardCapacity / 8;
                if (shardBytes * 8 != shardCapacity)
                {
                    // partially filled final byte
                    shardBytes++;
                }

                return shardBytes;
            }

            static void AssertExpectedLength(byte[] bytes, int shards, int shardCapacity)
            {
                var expectedBytes = (long)GetBytesCount(shardCapacity) * shards;
                expectedBytes += HeaderBytes; // + HeaderBytes for capacity values at start of array

                if (expectedBytes != bytes.Length)
                {
                    throw new ArgumentException($"Provided bytes had wrong length: expected {expectedBytes} but found {bytes.Length}");
                }
            }
        }
    }
}
