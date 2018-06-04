using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PwnedPasswords.Client
{
    /// <summary>
    /// Extensions for registering the <see cref="IPwnedPasswordsClient"/> with the <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedPasswordsClient"/> and a named <see cref="HttpClient"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
        /// <param name="configureClient">A delegate that is used to configure the <see cref="HttpClient"/> used by <see cref="IPwnedPasswordsClient"/></param>
        /// <returns>An <see cref="IHttpClientBuilder"/>that can be used to configure the client.</returns>
        /// <remarks>
        ///  <see cref="HttpClient"/> instances that apply the provided configuration can
        ///  be retrieved using <see cref="IHttpClientFactory.CreateClient(System.String)"/>
        ///  and providing the matching name.
        ///  <see cref="IPwnedPasswordsClient"/> instances constructed with the appropriate <see cref="HttpClient"/>
        ///  can be retrieved from <see cref="System.IServiceProvider.GetService(System.Type)"/> (and related
        ///  methods) by providing <see cref="IPwnedPasswordsClient"/> as the service type.
        /// </remarks>
        public static IHttpClientBuilder AddPwnedPasswordHttpClient(this IServiceCollection services, string name, Action<HttpClient> configureClient)
        {
            return services.AddHttpClient<IPwnedPasswordsClient, PwnedPasswordsClient>(name, configureClient);
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedPasswordsClient"/> and an <see cref="HttpClient"/>
        /// named <see cref="PwnedPasswordsClient.DefaultName"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureClient">A delegate that is used to configure the <see cref="HttpClient"/> used by <see cref="IPwnedPasswordsClient"/></param>
        /// <returns>An <see cref="IHttpClientBuilder"/>that can be used to configure the client.</returns>
        /// <remarks>
        ///  <see cref="HttpClient"/> instances that apply the provided configuration can
        ///  be retrieved using <see cref="IHttpClientFactory.CreateClient(System.String)"/>
        ///  and providing the matching name.
        ///  <see cref="IPwnedPasswordsClient"/> instances constructed with the appropriate <see cref="HttpClient"/>
        ///  can be retrieved from <see cref="System.IServiceProvider.GetService(System.Type)"/> (and related
        ///  methods) by providing <see cref="IPwnedPasswordsClient"/> as the service type.
        /// </remarks>
        public static IHttpClientBuilder AddPwnedPasswordHttpClient(this IServiceCollection services, Action<HttpClient> configureClient)
        {
            return services.AddPwnedPasswordHttpClient(PwnedPasswordsClient.DefaultName, configureClient);
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedPasswordsClient"/> and an <see cref="HttpClient"/>
        /// named <see cref="PwnedPasswordsClient.DefaultName"/> to use the public HaveIBeenPwned API 
        /// at "https://api.pwnedpasswords.com"
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>An <see cref="IHttpClientBuilder"/>that can be used to configure the client.</returns>
        /// <remarks>
        ///  <see cref="HttpClient"/> instances that apply the provided configuration can
        ///  be retrieved using <see cref="IHttpClientFactory.CreateClient(System.String)"/>
        ///  and providing the matching name.
        ///  <see cref="IPwnedPasswordsClient"/> instances constructed with the appropriate <see cref="HttpClient"/>
        ///  can be retrieved from <see cref="System.IServiceProvider.GetService(System.Type)"/> (and related
        ///  methods) by providing <see cref="IPwnedPasswordsClient"/> as the service type.
        /// </remarks>
        public static IHttpClientBuilder AddPwnedPasswordHttpClient(this IServiceCollection services)
        {
            return services.AddPwnedPasswordHttpClient(PwnedPasswordsClient.DefaultName, client =>
            {
                client.BaseAddress = new Uri("https://api.pwnedpasswords.com");
                client.DefaultRequestHeaders.Add("User-Agent", nameof(PwnedPasswordsClient));
            });
        }
    }
}
