using System;
using System.Threading.Tasks;
using HaveIBeenPwned;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;

namespace HaveIBeenPwnedValidator
{
    public class PwnedPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : class
    {
        private readonly IPwnedPasswordService _service;

        public PwnedPasswordValidator(IPwnedPasswordService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var isPwned = string.IsNullOrEmpty(password) || await _service.HasPasswordBeenPwned(password);

            var result = isPwned
                ? IdentityResult.Failed(new IdentityError
                {
                    Code = "PwnedPassword",
                    Description = "The password you chose has appeared in a data breach."
                })
                : IdentityResult.Success;

            return result;
        }
    }
}
