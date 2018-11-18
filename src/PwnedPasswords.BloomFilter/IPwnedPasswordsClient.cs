using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PwnedPasswords.BloomFilter
{
    /// <summary>
    /// A client for checking Troy Hunt's PwnedPasswords
    /// </summary>
    public interface IPwnedPasswordsClient
    {
        /// <summary>
        /// Checks if a provided password has appeared in a known data breach
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        /// <returns></returns>
        Task<bool> HasPasswordBeenPwned(string password, CancellationToken cancellationToken = default);
    }
}