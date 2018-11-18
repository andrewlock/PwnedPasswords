using PwnedPasswords.BloomFilter;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for registering the <see cref="IPwnedPasswordsClient"/> with the <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Loads a persisted filter from a file and adds a <see cref="IPwnedPasswordsClient"/> that uses the file. 
        /// The file is 
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="filePath">The file to load the persisted PwnedPasswords filter</param>
        /// <returns>The <see cref="IServiceCollection"/> for method chaining</returns>
        public static IServiceCollection AddPwnedPasswordsFromFilterFile(this IServiceCollection services, string filePath)
        {
            var filter = BloomFilter.Load(filePath);

            return services
                .AddSingleton<PwnedPasswordsClientFactory>()
                .AddSingleton<IPwnedPasswordsClient, PwnedPasswordsClient>(provider =>
                    provider.GetRequiredService<PwnedPasswordsClientFactory>().CreateFromSavedFilter(filePath));
        }
    }
}
