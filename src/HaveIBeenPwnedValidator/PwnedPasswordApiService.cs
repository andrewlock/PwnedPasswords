using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HaveIBeenPwnedValidator
{
    /// <summary>
    /// Uses the PwnedPasswords API to verify whether the password has been pwned
    /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
    /// If an error is returned, by the API, the password is assumed to be OK
    /// </summary>
    public class PwnedPasswordApiService : IPwnedPasswordService, IDisposable
    {
        private readonly PwnedPasswordApiServiceOptions _options;
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public PwnedPasswordApiService(ILogger<PwnedPasswordApiService> logger, PwnedPasswordApiServiceOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
        }

        /// <inheritdoc />
        public async Task<bool> HasPasswordBeenPwned(string password)
        {
            var sha1Password = SHA1Util.SHA1HashStringForUTF8String(password);
            var sha1Prefix = sha1Password.Substring(0, 5);
            var sha1Suffix = sha1Password.Substring(5);

            var msg = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}/{1}", _options.ApiUrl, sha1Prefix));

            var response = await _client.SendAsync(msg);

            return await HandleResponse(response, sha1Suffix);
        }

        protected async virtual Task<bool> HandleResponse(HttpResponseMessage response, string sha1Suffix)
        {
            if (response.IsSuccessStatusCode)
            {
                // Response was a success. Check to see if the SAH1 suffix is in the response body.
                var isPwned = (await response.Content.ReadAsStringAsync()).Contains(sha1Suffix);
                if (isPwned)
                {
                    _logger.LogDebug("HaveIBeenPwned API indicate the password has been pwned");
                    return true;
                }
                else
                {
                    _logger.LogDebug("HaveIBeenPwned API indicate the password has not been pwned");
                }
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogDebug("HaveIBeenPwned API indicate the password has not been pwned");
            }
            else
            {
                _logger.LogWarning("Unexepected response from API: {StatusCode}", response.StatusCode);
            }

            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client?.Dispose();
                    _semaphore?.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
        
    }
}
