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
                + 4 // BitsPerShard
                + 4 // NumberOfShards
                + 8 // TotalCapacity
                + 4; // ErrorRate

            public static BloomFilter Load(byte[] bytes)
            {
                if (bytes == null) throw new ArgumentNullException(nameof(bytes));
                if (bytes.Length < 7) throw new ArgumentException("The provided bytes are not a valid bloom filter");

                //read the number of hash functions
                Span<byte> span = bytes;
                var hashFunctionCount = span.Slice(0, 4).GetAsInt();
                var bitsPerShard = span.Slice(4, 4).GetAsInt();
                var numberOfShards = span.Slice(8, 4).GetAsInt();
                var totalCapacity = span.Slice(12, 8).GetAsLong();
                var errorRate = span.Slice(20, 4).GetAsFloat();

                AssertExpectedLength(bytes, numberOfShards, bitsPerShard);

                var index = HeaderBytes;
                var shards = new BitArray[numberOfShards];
                var bytesPerShard = GetBytesCount(bitsPerShard);

                for (int i = 0; i < numberOfShards; i++)
                {
                    var slicedBytes = span.Slice(index, bytesPerShard).ToArray();
                    shards[i] = new BitArray(slicedBytes) { Length = bitsPerShard };
                    index += bytesPerShard;
                }

                return new BloomFilter(shards, hashFunctionCount,bitsPerShard, totalCapacity, errorRate);
            }

            public static byte[] Save(BloomFilter filter)
            {
                var bytesPerShard = GetBytesCount(filter.BitsPerShard);
                var arrayLength = (long)bytesPerShard * filter.Shards.Count + HeaderBytes;
                var data = new byte[arrayLength];

                // save the headers
                filter.HashFunctionCount.GetAsBytes().CopyTo(data, 0);
                filter.BitsPerShard.GetAsBytes().CopyTo(data, 4);
                filter.Shards.Count.GetAsBytes().CopyTo(data, 8);
                filter.TotalCapacity.GetAsBytes().CopyTo(data, 12);
                filter.ExpectedErrorRate.GetAsBytes().CopyTo(data, 20);

                // now copy the actual data
                var index = HeaderBytes;
                foreach (var hashBits in filter.Shards)
                {
                    hashBits.CopyTo(data, index);
                    index += bytesPerShard;
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
