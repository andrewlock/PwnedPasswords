using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PwnedPasswords.BloomFilter.Test
{
    public class BloomFilterSerializerTests
    {
        [Fact]
        public void CanRoundTripWithCapacityDivisibleBy8()
        {
            var requiredCapacity = 128;
            var filter = new BloomFilter(requiredCapacity);
            var expectedCapacity = filter.HashBits.Count;
            var expectedHashFunctions = filter.HashFunctionCount;

            var bytes = filter.Save();
            var newFilter = BloomFilter.Load(bytes);

            Assert.Equal(expectedHashFunctions, newFilter.HashFunctionCount);
            Assert.Equal(expectedCapacity, newFilter.HashBits.Length);
            Assert.Equal(filter.Capacity, newFilter.Capacity);
            Assert.Equal(filter.ExpectedErrorRate, newFilter.ExpectedErrorRate);
        }

        [Fact]
        public void CanRoundTripWithCapacityDivisibleNotBy8()
        {
            var requiredCapacity = 129;
            var filter = new BloomFilter(requiredCapacity);
            var expectedCapacity = filter.HashBits.Count;
            var expectedHashFunctions = filter.HashFunctionCount;

            var bytes = filter.Save();
            var newFilter = BloomFilter.Load(bytes);

            Assert.Equal(expectedHashFunctions, newFilter.HashFunctionCount);
            Assert.Equal(expectedCapacity, newFilter.HashBits.Length);
            Assert.Equal(filter.Capacity, newFilter.Capacity);
            Assert.Equal(filter.ExpectedErrorRate, newFilter.ExpectedErrorRate);
        }

        [Fact]
        public void SameFilterCreatedTwiceIsIdentical()
        {
            var requiredCapacity = 128;
            var filter = new BloomFilter(requiredCapacity);
            var inputs = GenerateRandomDataList(requiredCapacity);
            filter.AddRange(inputs);

            var bytes = filter.Save();
            var originalBytes = new byte[bytes.Length];
            bytes.CopyTo(originalBytes, 0);

            var newFilter = new BloomFilter(requiredCapacity);
            newFilter.AddRange(inputs);
            var newBytes = newFilter.Save();

            Assert.Equal(bytes.Length, newBytes.Length);
            for (var i = 0; i < bytes.Length; i++)
            {
                Assert.True(bytes[i] == newBytes[i], $"Expected byte {i} to be {bytes[i]}, was{newBytes[i]}");
            }
        }

        [Fact]
        public void SameFilterCreatedTwiceWithErrorRateIsIdentical()
        {
            var requiredCapacity = 128;
            var errorRate = 0.001F; // 0.1%
            var filter = new BloomFilter(requiredCapacity, errorRate);
            var inputs = GenerateRandomDataList(requiredCapacity);
            filter.AddRange(inputs);

            var bytes = filter.Save();
            var originalBytes = new byte[bytes.Length];
            bytes.CopyTo(originalBytes, 0);

            var newFilter = new BloomFilter(requiredCapacity, errorRate);
            newFilter.AddRange(inputs);
            var newBytes = newFilter.Save();

            Assert.Equal(bytes.Length, newBytes.Length);
            for (var i = 0; i < bytes.Length; i++)
            {
                Assert.True(bytes[i] == newBytes[i], $"Expected byte {i} to be {bytes[i]}, was{newBytes[i]}");
            }
        }

        [Fact]
        public void BytesAfterRoundTripAreIdentical()
        {
            var requiredCapacity = 128;
            var filter = new BloomFilter(requiredCapacity);
            var inputs = GenerateRandomDataList(requiredCapacity);
            filter.AddRange(inputs);

            var bytes = filter.Save();
            var originalBytes = new byte[bytes.Length];
            bytes.CopyTo(originalBytes, 0);

            var newFilter = BloomFilter.Load(bytes);
            var newBytes = newFilter.Save();

            Assert.Equal(originalBytes.Length, newBytes.Length);
            for (var i = 0; i < originalBytes.Length; i++)
            {
                Assert.True(originalBytes[i] == newBytes[i], $"Expected byte {i} to be {originalBytes[i]}, was{newBytes[i]}");
            }
        }

        [Fact]
        public void AfterRoundTripHasSameBehaviour()
        {
            // set filter properties
            var capacity = 10000;
            var errorRate = 0.001F; // 0.1%

            // create input collection
            var inputs = GenerateRandomDataList(capacity);

            // instantiate filter and populate it with the inputs
            var target = new BloomFilter(capacity, errorRate);
            target.AddRange(inputs);

            // check for each input
            foreach (var input in inputs)
            {
                Assert.True(target.Contains(input), $"False negative: {input}");
            }

            // round trip the filter
            var bytes = target.Save();
            var newFilter = BloomFilter.Load(bytes);

            // check again for each input
            foreach (var input in inputs)
            {
                Assert.True(newFilter.Contains(input), $"False negative: {input}");
            }
        }

        [Fact]
        public void AfterRoundTripToFileHasSameBehaviour()
        {
            // set filter properties
            var filePath = $"testdata-{Guid.NewGuid()}.filter";
            var capacity = 100_000;
            var errorRate = 0.001F; // 0.1%

            // create input collection
            var inputs = GeneratePseudoRandomDataList(capacity);

            // instantiate filter and populate it with the inputs
            var target = new BloomFilter(capacity, errorRate);
            foreach (var input in inputs)
            {
                target.Add(input);
            }

            // check for each input
            foreach (var input in inputs)
            {
                Assert.True(target.Contains(input), $"False negative: {input}");
            }

            // round trip the filter
            target.Save(filePath);
            var newFilter = BloomFilter.Load(filePath);

            // check again for each input
            foreach (var input in inputs)
            {
                Assert.True(newFilter.Contains(input), $"False negative: {input}");
            }

            var newFilter2 = BloomFilter.Load(filePath);

            // check again for each input
            foreach (var input in inputs)
            {
                Assert.True(newFilter2.Contains(input), $"False negative: {input}");
            }
        }

        [Fact]
        public void CanSaveToFile()
        {
            var filePath = $"testdata-{Guid.NewGuid()}.filter";
            // set filter properties
            var capacity = 10000;
            var errorRate = 0.001F; // 0.1%

            // create input collection
            var inputs = GeneratePseudoRandomDataList(capacity);

            // instantiate filter and populate it with the inputs
            var filter = new BloomFilter(capacity, errorRate);
            foreach (var input in inputs)
            {
                filter.Add(input);
            }

            filter.Save(filePath);
        }

        [Fact]
        public void CanLoadFromFileInDifferentProcessToSavedFile()
        {
            // set filter properties
            var filePath = "testdata.filter";
            var capacity = 10000;

            // create input collection
            var inputs = GeneratePseudoRandomDataList(capacity);
            var savedFilter = BloomFilter.Load(filePath);

            Assert.Equal(inputs.Count, savedFilter.Capacity);
            // check again for each input
            foreach (var input in inputs)
            {
                Assert.True(savedFilter.Contains(input), $"False negative: {input}");
            }
        }

        [Fact]
        public void PseudoRandomListIsAlwaysIdentical()
        {
            // Use pre-existing filter
            var capacity = 10000;
            var inputs1 = GeneratePseudoRandomDataList(capacity);
            var inputs2 = GeneratePseudoRandomDataList(capacity);

            Assert.Equal(inputs1, inputs2);
        }

        private static List<string> GenerateRandomDataList(int capacity)
        {
            var inputs = new List<string>(capacity);
            for (var i = 0; i < capacity; i++)
            {
                inputs.Add(Guid.NewGuid().ToString());
            }
            return inputs;
        }

        private static List<string> GeneratePseudoRandomDataList(int capacity)
        {
            const int seed = 123456;
            var r = new Random(seed);
            var inputs = new List<string>(capacity);

            for (var i = 0; i < capacity; i++)
            {
                var guid = new byte[16];
                r.NextBytes(guid);
                inputs.Add(new Guid(guid).ToString());
            }
            return inputs;
        }
    }
}
