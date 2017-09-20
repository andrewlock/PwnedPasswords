namespace HaveIBeenPwned
{
    /// <summary>
    /// Options for the <see cref="PwnedPasswordApiService"/>
    /// </summary>
    public class PwnedPasswordApiServiceOptions
    {
        /// <summary>
        /// The user-agent used to call the HaveIBeenPwned API
        /// Defaults to the full name of <see cref="PwnedPasswordApiService"/>
        /// </summary>
        public string UserAgent { get; set; } = typeof(PwnedPasswordApiService).FullName;

        /// <summary>
        /// The full URL of the API to call
        /// Defaults to https://haveibeenpwned.com/api/v2/pwnedpassword
        /// </summary>
        public string ApiUrl { get; set; } = "https://haveibeenpwned.com/api/v2/pwnedpassword";

        /// <summary>
        /// The minimum time between requests to the API in milliseconds
        /// Defaults to 1,500ms
        /// </summary>
        public int RateLimiteMilliSeconds { get; set; } = 1_500;
    }
}
