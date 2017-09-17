using System.Threading.Tasks;

namespace HaveIBeenPwnedValidator
{
    /// <summary>
    /// Checks whether provided passwords have been pwned
    /// </summary>
    public interface IPwnedPasswordService
    {
        /// <summary>
        /// If the provided password has been Pwned, returns <code>true</code>
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> HasPasswordBeenPwned(string password);
    }
}