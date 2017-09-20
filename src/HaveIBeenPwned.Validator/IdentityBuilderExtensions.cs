using System;
using HaveIBeenPwned;
using HaveIBeenPwnedValidator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordApiValidator<TUser>(this IdentityBuilder builder) where TUser : class
        {
            return builder.AddPwnedPasswordApiValidator<TUser>(configure: null);
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="configure">Configure the options for the API service</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordApiValidator<TUser>(this IdentityBuilder builder, Action<PwnedPasswordApiServiceOptions> configure) where TUser : class
        {
            builder.Services.AddPwnedPasswordApiService(configure);
            return builder.AddPasswordValidator<PwnedPasswordValidator<TUser>>();
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password by checking the provided files
        /// The files should be relative to the <see cref="IHostingEnvironment.ContentRootPath"/>
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="filename">The filename containing the SHA1 hashes of pwned passwords</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordFileValidator<TUser>(this IdentityBuilder builder, string filename) where TUser : class
        {
            return builder.AddPwnedPasswordFileValidator<TUser>(options => options.Filenames = new[] { filename });
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password by checking the provided files
        /// The files should be relative to the <see cref="IHostingEnvironment.ContentRootPath"/>
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="filenames">The filenames containing the SHA1 hashes of pwned passwords</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordFileValidator<TUser>(this IdentityBuilder builder, params string[] filenames) where TUser : class
        {
            return builder.AddPwnedPasswordFileValidator<TUser>(options => options.Filenames = filenames);
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password by checking the provided files
        /// The files should be relative to the <see cref="IHostingEnvironment.ContentRootPath"/>
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="configure">Configure the options for the file checking service</param>
        /// <typeparam name="TUser">The user type whose password will be validated.</typeparam>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IdentityBuilder AddPwnedPasswordFileValidator<TUser>(this IdentityBuilder builder, Action<PwnedPasswordFileServiceOptions> configure) where TUser : class
        {
            builder.Services.AddPwnedPasswordFileService(configure);
            return builder.AddPasswordValidator<PwnedPasswordValidator<TUser>>();
        }
    }
}
