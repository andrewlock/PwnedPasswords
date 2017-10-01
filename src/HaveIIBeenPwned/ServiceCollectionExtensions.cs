using System;
using HaveIBeenPwned;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an instance of <see cref="PwnedPasswordApiService"/> to the service collection
        /// </summary>
        /// <param name="services">The services collection instance this method extends</param>
        /// <param name="configure">Configure the options for the API service</param>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IServiceCollection AddPwnedPasswordApiService(this IServiceCollection services, Action<PwnedPasswordApiServiceOptions> configure)
        {
            var options = new PwnedPasswordApiServiceOptions();
            if (configure != null)
            {
                configure(options);
            }
            services.AddSingleton(options);
            services.AddSingleton<IPwnedPasswordService, PwnedPasswordApiService>();
            return services;
        }

        /// <summary>
        /// Adds an instance of <see cref="PwnedPasswordFileService"/> to the service collection
        /// </summary>
        /// <param name="builder">The Microsoft.AspNetCore.Identity.IdentityBuilder instance this method extends</param>
        /// <param name="configure">Configure the options for the file checking service</param>
        /// <returns>The current Microsoft.AspNetCore.Identity.IdentityBuilder instance.</returns>
        public static IServiceCollection AddPwnedPasswordFileService(this IServiceCollection services, Action<PwnedPasswordFileServiceOptions> configure)
        {
            var options = new PwnedPasswordFileServiceOptions();
            if (configure != null)
            {
                configure(options);
            }
            services.AddSingleton(options);
            services.AddSingleton<IPwnedPasswordService, PwnedPasswordFileService>();
            return services;
        }
    }
}
