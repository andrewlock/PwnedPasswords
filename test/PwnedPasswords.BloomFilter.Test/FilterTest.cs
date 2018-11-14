// Source code adapted from https://archive.codeplex.com/?p=bloomfilter#BloomFilter/Filter.cs
using System;
using System.Collections.Generic;
using Xunit;

namespace PwnedPasswords.BloomFilter.Test
{
    public class FilterTest
    {
        /// <summary>
        /// There should be no false negatives.
        /// </summary>
        [Fact]
        public void NoFalseNegativesTest()
        {
            // set filter properties
            var capacity = 10000;
            var errorRate = 0.001F; // 0.1%

            // create input collection
            var inputs = GenerateRandomDataList(capacity);

            // instantiate filter and populate it with the inputs
            var target = new BloomFilter(capacity, errorRate);
            foreach (var input in inputs)
            {
                target.Add(input);
            }

            // check for each input. if any are missing, the test failed
            foreach (var input in inputs)
            {
                Assert.True(target.Contains(input), $"False negative: {input}");
            }
        }

        /// <summary>
        /// Only in extreme cases should there be a false positive with this test.
        /// </summary>
        [Fact]
        public void LowProbabilityFalseTest()
        {
            var capacity = 10000; // we'll actually add only one item
            var errorRate = 0.0001F; // 0.01%

            // instantiate filter and populate it with a single random value
            var target = new BloomFilter(capacity, errorRate);
            target.Add(Guid.NewGuid().ToString());

            // generate a new random value and check for it
            Assert.False(target.Contains(Guid.NewGuid().ToString()), "Check for missing item returned true.");
        }

        [Fact]
        public void FalsePositivesInRangeTest()
        {
            // set filter properties
            var capacity = 1000000;
            var errorRate = 0.001F; // 0.1%

            // instantiate filter and populate it with random strings
            var target = new BloomFilter(capacity, errorRate);
            for (var i = 0; i < capacity; i++)
            {
                target.Add(Guid.NewGuid().ToString());
            }

            // generate new random strings and check for them
            // about errorRate of them should return positive
            var falsePositives = 0;
            var testIterations = capacity;
            var expectedFalsePositives = ((int)(testIterations * errorRate)) * 2;
            for (var i = 0; i < testIterations; i++)
            {
                var test = Guid.NewGuid().ToString();
                if (target.Contains(test) == true)
                {
                    falsePositives++;
                }
            }

            Assert.True(falsePositives <= expectedFalsePositives,
                $"Number of false positives ({falsePositives}) greater than expected ({expectedFalsePositives}).");
        }

        [Fact]
        public void OverLargeInputTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // set filter properties
                var capacity = int.MaxValue - 1;
                var errorRate = 0.01F; // 1%

                // instantiate filter
                var target = new BloomFilter(capacity, errorRate);
            });
        }

        [Fact]
        public void LargeInputTest()
        {
            // set filter properties
            var capacity = 2000000;
            var errorRate = 0.01F; // 1%

            // instantiate filter and populate it with random strings
            var target = new BloomFilter(capacity, errorRate);
            for (var i = 0; i < capacity; i++)
            {
                target.Add(Guid.NewGuid().ToString());
            }

            // if it didn't error out on that much input, this test succeeded
        }

        [Fact]
        public void LargeInputTestAutoError()
        {
            // set filter properties
            var capacity = 2000000;

            // instantiate filter and populate it with random strings
            var target = new BloomFilter(capacity);
            for (var i = 0; i < capacity; i++)
            {
                target.Add(Guid.NewGuid().ToString());
            }

            // if it didn't error out on that much input, this test succeeded
        }

        /// <summary>
        /// If k and m are properly chosen for n and the error rate, the filter should be about half full.
        /// </summary>
        [Fact]
        public void TruthinessTest()
        {
            var capacity = 10000;
            var errorRate = 0.001F; // 0.1%
            var target = new BloomFilter(capacity, errorRate);
            for (var i = 0; i < capacity; i++)
            {
                target.Add(Guid.NewGuid().ToString());
            }

            var actual = target.Truthiness;
            var expected = 0.5;
            var threshold = 0.01; // filter shouldn't be < 49% or > 51% "true"
            var difference = Math.Abs(actual - expected);
            Assert.True(difference < threshold, $"Information density too high or low. Actual={actual}, Expected={expected}");
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

        [Fact]
        public void InvalidCapacityConstructorTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var errorRate = 0.1F;
                var capacity = 0; // no good
                var target = new BloomFilter(capacity, errorRate);
            });
        }

        [Fact]
        public void InvalidErrorRateConstructorTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var errorRate = 10F; // no good
                var capacity = 10;
                var target = new BloomFilter(capacity, errorRate);
            });
        }
    }
}
