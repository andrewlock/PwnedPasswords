namespace PwnedPasswords.Client
{
    /// <summary>
    /// Options for configuring the <see cref="IPwnedPasswordsClient"/>
    /// </summary>
    public class PwnedPasswordsClientOptions
    {
        /// <summary>
        /// The minimum frequency to consider a password to be Pwned.
        /// For example, setting this to 20 means only PwnedPasswords seen 20 times
        /// or more will be considered Pwned. Defaults to 1 (so all pwned passwords are considered)
        /// </summary>
        public int MinimumFrequencyToConsiderPwned { get; set; } = 1;

        /// <summary>
        /// If true, requests for the API to add padding to the response.
        /// This adds an additional layer of privacy, but uses more bandwidth. Defaults to false.
        /// See the blog post for details: https://www.troyhunt.com/enhancing-pwned-passwords-privacy-with-padding/ 
        /// </summary>
        public bool AddPadding { get; set; } = false;
    }
}
