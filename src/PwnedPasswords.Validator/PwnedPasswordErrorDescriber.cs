using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace PwnedPasswords.Validator
{
    /// <summary>
    /// Service to enable localization for application facing validation errors.
    /// </summary>
    /// <remarks>
    /// These errors are returned to controllers and are generally used as display messages to end users.
    /// </remarks>
    public class PwnedPasswordErrorDescriber
    {
        private readonly PwnedPasswordValidatorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PwnedPasswordErrorDescriber"/> class.
        /// </summary>
        /// <param name="options">The validator options.</param>
        public PwnedPasswordErrorDescriber(IOptions<PwnedPasswordValidatorOptions> options = null)
        {
            _options = options?.Value;
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a pwned password was identified.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a pwned password was identified.</returns>
        public virtual IdentityError PwnedPassword()
        {
            return new IdentityError
            {
                Code = nameof(PwnedPassword),
                Description = _options?.ErrorMessage
            };
        }
    }
}