using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
        private readonly PwnedPasswordValidatorOptions _options;

        /// <summary>
        /// Create a new instance of the <see cref="PwnedPasswordValidator{TUser}"/>
        /// </summary>
        public PwnedPasswordValidator(IPwnedPasswordsClient client, IOptions<PwnedPasswordValidatorOptions> options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var isPwned = false;
            if (!string.IsNullOrEmpty(password))
            {
                isPwned = await _client.HasPasswordBeenPwned(password);
            }

            var result = isPwned
                ? IdentityResult.Failed(new IdentityError
                {
                    Code = "PwnedPassword",
                    Description = _options.ErrorMessage,
                })
                : IdentityResult.Success;

            return result;
        }
    }
}
