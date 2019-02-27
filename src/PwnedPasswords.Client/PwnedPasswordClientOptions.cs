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
    }
}
