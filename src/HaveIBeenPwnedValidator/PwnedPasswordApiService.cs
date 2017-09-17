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

            var formContent = new FormUrlEncodedContent(
                new Dictionary<string, string> { { "Password", sha1Password } });

            var msg = new HttpRequestMessage(HttpMethod.Post, _options.ApiUrl)
            {
                Content = formContent,
            };

            var response = await ThrottleRequest(() => _client.SendAsync(msg));

            return HandleResponse(response);
        }

        private async Task<T> ThrottleRequest<T>(Func<Task<T>> action)
        {
            //wait for the semaphore to be available
            await _semaphore.WaitAsync();

            //run the task
            var response = await action();

            // This doesn't look right, but can't think what I should be doing at the moment. Probably needs to be a queue
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                // wait on a background thread for the min required time
                Thread.Sleep(_options.RateLimiteMilliSeconds);
                _semaphore.Release();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return response;
        }

        protected virtual bool HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("HaveIBeenPwned API indicate the password has been pwned");
                return true;
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
