using System;

namespace PwnedPasswords.BloomFilter
{
    /// <summary>
    /// An exception raised when the file does not have the expected format
    /// </summary>
    public class InvalidFileFormatException : Exception
    {
        /// <summary>
        /// Creates an instance of the exception
        /// </summary>
        /// <param name="message"></param>
        public InvalidFileFormatException(string message) : base(message)
        {
        }
    }
}
