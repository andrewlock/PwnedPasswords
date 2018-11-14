// Source code adapted from https://archive.codeplex.com/?p=bloomfilter
using System;
using System.Collections;

namespace PwnedPasswords.BloomFilter
{
    /// <summary>
    /// A bloom filter used to hash strings
    /// </summary>
    public class BloomFilter
    {
        /// <summary>
        /// Creates a new Bloom filter, specifying an error rate of 1/capacity, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
        /// A secondary hash function will be provided for you if your type T is either string or int. Otherwise an exception will be thrown. If you are not using these types please use the overload that supports custom hash functions.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        public BloomFilter(int capacity) : this(capacity, BestErrorRate(capacity)) { }

        /// <summary>
        /// Creates a new Bloom filter, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
        /// A secondary hash function will be provided for you if your type T is either string or int. Otherwise an exception will be thrown. If you are not using these types please use the overload that supports custom hash functions.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        /// <param name="errorRate">The acceptable false-positive rate (e.g., 0.01F = 1%)</param>
        public BloomFilter(int capacity, float errorRate) : this(capacity, errorRate, BestM(capacity, errorRate), BestK(capacity, errorRate)) { }

        /// <summary>
        /// Creates a new Bloom filter.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        /// <param name="errorRate">The acceptable false-positive rate (e.g., 0.01F = 1%)</param>
        /// <param name="m">The number of elements in the BitArray.</param>
        /// <param name="k">The number of hash functions to use.</param>
        public BloomFilter(int capacity, float errorRate, int m, int k)
        {
            // validate the params are in range
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "capacity must be > 0");
            }

            if (errorRate >= 1 || errorRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(errorRate), errorRate,
                    $"errorRate must be between 0 and 1, exclusive. Was {errorRate}");
            }

            if (m < 1) // from overflow in bestM calculation
            {
                throw new ArgumentOutOfRangeException(
                    $"The provided capacity and errorRate values would result in an array of length > int.MaxValue. Please reduce either of these values. Capacity: {capacity}, Error rate: {errorRate}");
            }

            HashFunctionCount = k;
            HashBits = new BitArray(m);
        }

        /// <summary>
        /// Adds a new item to the filter. It cannot be removed.
        /// </summary>
        /// <param name="item"></param>
        public void Add(string item)
        {
            // start flipping bits for each hash of item
            var primaryHash = item.GetHashCode();
            var secondaryHash = HashString(item);
            for (var i = 0; i < HashFunctionCount; i++)
            {
                var hash = ComputeHash(primaryHash, secondaryHash, i);
                HashBits[hash] = true;
            }
        }

        /// <summary>
        /// Checks for the existence of the item in the filter for a given probability.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(string item)
        {
            var primaryHash = item.GetHashCode();
            var secondaryHash = HashString(item);
            for (var i = 0; i < HashFunctionCount; i++)
            {
                var hash = ComputeHash(primaryHash, secondaryHash, i);
                if (HashBits[hash] == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// The ratio of false to true bits in the filter. E.g., 1 true bit in a 10 bit filter means a truthiness of 0.1.
        /// </summary>
        public double Truthiness => (double) TrueBits() / HashBits.Count;

        private int TrueBits()
        {
            var output = 0;
            foreach (bool bit in HashBits)
            {
                if (bit == true)
                {
                    output++;
                }
            }
            return output;
        }

        /// <summary>
        /// Performs Dillinger and Manolios double hashing. 
        /// </summary>
        private int ComputeHash(int primaryHash, int secondaryHash, int i)
        {
            var resultingHash = (primaryHash + (i * secondaryHash)) % HashBits.Count;
            return Math.Abs((int)resultingHash);
        }

        /// <summary>
        /// Get the number of hash functions used to create the filter
        /// </summary>
        public int HashFunctionCount { get; }
        
        /// <summary>
        /// Get the filter as a bit array
        /// </summary>
        public BitArray HashBits { get; }

        private static int BestK(int capacity, float errorRate)
        {
            return (int)Math.Round(Math.Log(2.0) * BestM(capacity, errorRate) / capacity);
        }

        private static int BestM(int capacity, float errorRate)
        { 
            return (int)Math.Ceiling(capacity * Math.Log(errorRate, (1.0 / Math.Pow(2, Math.Log(2.0)))));
        }

        private static float BestErrorRate(int capacity)
        {
            var c = (float)(1.0 / capacity);
            if (c != 0)
            {
                return c;
            }
            else
            {
                return (float)Math.Pow(0.6185, int.MaxValue / capacity); // http://www.cs.princeton.edu/courses/archive/spring02/cs493/lec7.pdf
            }
        }
        
        /// <summary>
        /// Hashes a string using Bob Jenkin's "One At A Time" method from Dr. Dobbs (http://burtleburtle.net/bob/hash/doobs.html).
        /// Runtime is suggested to be 9x+9, where x = input.Length. 
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>The hashed result.</returns>
        private static int HashString(string input)
        {
            var hash = 0;

            for (var i = 0; i < input.Length; i++)
            {
                hash += input[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }
    }
}