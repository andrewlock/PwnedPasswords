using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using PwnedPasswords.Client;
using PwnedPasswords.Validator;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for adding <see cref="PwnedPasswordValidator{TUser}"/> to an <see cref="IdentityBuilder"/>
    /// </summary>
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordValidator<TUser>(this IdentityBuilder builder) where TUser : class
        {
            return builder.AddPwnedPasswordValidator<TUser>(configure: opts => { });
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="configure">Configure the options for the Validator</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordValidator<TUser>(
            this IdentityBuilder builder, Action<PwnedPasswordValidatorOptions> configure) 
            where TUser : class
        {
            builder.Services.Configure<PwnedPasswordValidatorOptions>(configure);
            return builder.AddPasswordValidator<PwnedPasswordValidator<TUser>>();
        }
    }
}
