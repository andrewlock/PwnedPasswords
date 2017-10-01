using System.Threading.Tasks;

namespace HaveIBeenPwned
{
    /// <summary>
    /// Checks whether provided passwords have been pwned
    /// </summary>
    public interface IPwnedPasswordService
    {
        /// <summary>
        /// If the provided plain-text password has been Pwned, returns <code>true</code>
        /// </summary>
        /// <param name="plainTextPassword">The plain text password to look up, will be hashed</param>
        /// <returns></returns>
        Task<bool> HasPasswordBeenPwned(string plainTextPassword);

        /// <summary>
        /// If the provided sha1-hashed password has been Pwned, returns <code>true</code>
        /// </summary>
        /// <param name="sha1password">The SHA1 of a password to look up</param>
        /// <returns></returns>
        Task<bool> HasSha1PasswordBeenPwned(string sha1password);
    }
}