using System;
using System.Runtime.InteropServices;

namespace PwnedPasswords.BloomFilter
{
    internal static class ConversionHelper
    {
        public static byte[] GetAsBytes(this int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        public static byte[] GetAsBytes(this float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        public static int GetAsInt(this Span<byte> data)
        {
            if (BitConverter.IsLittleEndian)
            {
                // data is stored big endian, so if we're little endian we need to reverse the order
                data.Reverse();
            }

            var bytesAsInt = MemoryMarshal.Cast<byte, int>(data);
            return bytesAsInt[0];
        }

        public static float GetAsFloat(this Span<byte> data)
        {
            if (BitConverter.IsLittleEndian)
            {
                // data is stored big endian, so if we're little endian we need to reverse the order
                data.Reverse();
            }

            var bytesAsFloat = MemoryMarshal.Cast<byte, float>(data);
            return bytesAsFloat[0];
        }
    }
}