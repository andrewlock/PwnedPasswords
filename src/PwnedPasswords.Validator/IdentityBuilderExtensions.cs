using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddPwnedPasswordValidator<TUser>(
            this IdentityBuilder builder, Action<PwnedPasswordValidatorOptions> configure)
            where TUser : class
        {
            return builder.AddPwnedPasswordValidator<TUser>(configure, configureClient: opts => { });
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="configure">Configure the options for the Validator</param>
        /// <param name="configureClient">A delegate that is used to configure the <see cref="PwnedPasswordsClientOptions"/></param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddPwnedPasswordValidator<TUser>(
            this IdentityBuilder builder, Action<PwnedPasswordValidatorOptions> configure,
            Action<PwnedPasswordsClientOptions> configureClient)
            where TUser : class
        {
            if (!builder.Services.Any(x => x.ServiceType == typeof(IPwnedPasswordsClient)))
            {
                builder.Services.AddPwnedPasswordHttpClient();
            }
            builder.Services.Configure(configure);
            builder.Services.Configure(configureClient);
            builder.Services.TryAddScoped<PwnedPasswordErrorDescriber>();
            return builder.AddPasswordValidator<PwnedPasswordValidator<TUser>>();
        }

        /// <summary>
        /// Adds a <see cref="PwnedPasswordErrorDescriber"/>
        /// </summary>
        /// <typeparam name="TDescriber">The type of the error describer.</typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance.</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddPwnedPasswordErrorDescriber<TDescriber>(this IdentityBuilder builder) 
            where TDescriber : PwnedPasswordErrorDescriber
        {
            builder.Services.AddScoped<PwnedPasswordErrorDescriber, TDescriber>();
            return builder;
        }
    }
}
