using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PwnedPasswords.BloomFilter.Test
{
    public class SpanUtilTests
    {
        [Theory]
        [InlineData('1', 1)]
        [InlineData('0', 0)]
        [InlineData('a', 10)]
        [InlineData('f', 15)]
        [InlineData('A', 10)]
        [InlineData('F', 15)]
        public void CanGetHexValue(char hex, int expected)
        {
            var actual = SpanUtil.GetHexVal(hex);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("7C4A8D09", 2085260553)]
        [InlineData("CA3762AF", -902339921)] // uint = 3392627375
        [InlineData("7c4a8d09", 2085260553)]
        [InlineData("ca3762af", -902339921)] // uint = 3392627375
        public void CanParseHexStringIntoInt(string hex, int expected)
        {
            var actual = SpanUtil.GetHash(hex.AsSpan());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanGetPrimaryHashFromFullSha()
        {
            const string sha = "7C4A8D09CA3762AF61E59520943DC26494F8941B";
            const int expected = 2085260553;
            var actual = SpanUtil.GetPrimaryHash(sha.AsSpan());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanGetSecondaryHashFromFullSha()
        {
            const string sha = "7C4A8D09CA3762AF61E59520943DC26494F8941B";
            const int expected = -902339921;
            var actual = SpanUtil.GetSecondaryHash(sha.AsSpan());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NoErrorsForManyShas()
        {
            for (int i = 0; i < 100_000; i++)
            {
                var sha = Guid.NewGuid().ToString("N");
                SpanUtil.GetPrimaryHash(sha.AsSpan());
                SpanUtil.GetSecondaryHash(sha.AsSpan());
            }
        }
    }
}
