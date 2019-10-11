using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PwnedPasswords.Client;

namespace PwnedPasswords.Validator
{
    /// <summary>
    /// An <see cref="IPasswordValidator{TUser}"/> for verifying a given password has not appeared in a data breach
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class PwnedPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : class
    {
        private readonly IPwnedPasswordsClient _client;

        /// <summary>
        /// Create a new instance of the <see cref="PwnedPasswordValidator{TUser}"/>
        /// </summary>
        public PwnedPasswordValidator(IPwnedPasswordsClient client, PwnedPasswordErrorDescriber describer)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Describer = describer ?? throw new ArgumentNullException(nameof(describer));
        }

        /// <summary>
        /// Gets the <see cref="PwnedPasswordErrorDescriber"/> used to supply error text.
        /// </summary>
        /// <value>
        /// The <see cref="PwnedPasswordErrorDescriber"/> used to supply error text.
        /// </value>
        public PwnedPasswordErrorDescriber Describer { get; }

        /// <inheritdoc />
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var isPwned = false;
            if (!string.IsNullOrEmpty(password))
            {
                isPwned = await _client.HasPasswordBeenPwned(password);
            }

            var result = isPwned
                ? IdentityResult.Failed(Describer.PwnedPassword())
                : IdentityResult.Success;

            return result;
        }
    }
}
