using System.Threading;
using System.Threading.Tasks;

namespace PwnedPasswords.BloomFilter
{
    /// <inheritdoc />
    public class PwnedPasswordsClient : IPwnedPasswordsClient
    {
        private readonly BloomFilter _bloomFilter;

        internal PwnedPasswordsClient(BloomFilter bloomFilter)
        {
            _bloomFilter = bloomFilter;
        }

        /// <inheritdoc />
        public Task<bool> HasPasswordBeenPwned(string password, CancellationToken cancellationToken = default)
        {
            var sha1Password = SHA1Util.SHA1HashStringForUTF8String(password);
            return Task.FromResult(_bloomFilter.Contains(sha1Password));
        }
    }
}
