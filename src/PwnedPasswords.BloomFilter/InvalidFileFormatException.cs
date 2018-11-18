using System;

namespace PwnedPasswords.BloomFilter
{
    public class InvalidFileFormatException : Exception
    {
        public InvalidFileFormatException(string message) : base(message)
        {
        }
    }
}
