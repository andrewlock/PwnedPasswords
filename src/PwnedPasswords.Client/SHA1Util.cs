using System.Security.Cryptography;
using System.Text;

namespace PwnedPasswords.Client
{
    /// <summary>
    /// Utilities for generating a SHA1 hash
    /// </summary>
    public static class SHA1Util
    {
        private static readonly SHA1 _sha1 = SHA1.Create();

        /// <summary>
        /// Compute hash for string
        /// </summary>
        /// <param name="s">String to be hashed</param>
        /// <returns>40-character hex string</returns>
        public static string SHA1HashStringForUTF8String(string s)
        {
            byte[] bytes = Encoding.Default.GetBytes(s);

            byte[] hashBytes = _sha1.ComputeHash(bytes);

            return HexStringFromBytes(hashBytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("X2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}
