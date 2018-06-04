using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PwnedPasswords.Client.Test
{
    public class HaveIBeenPwnedClientTests
    {
        [Fact, Trait("Category", "Integration")] // don't run it automatically
        public async Task HasPasswordBeenPwned_WhenStrongPassword_ReturnsFalse()
        {
            PwnedPasswordsClient service = GetClient();

            var safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwned(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");
        }

        [Fact, Trait("Category", "Integration")] // don't run it automatically
        public async Task HasPasswordBeenPwned_WhenWeakPassword_ReturnsTrue()
        {
            PwnedPasswordsClient service = GetClient();;

            var pwnedPassword = "password";

            var isPwned = await service.HasPasswordBeenPwned(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        private static PwnedPasswordsClient GetClient()
        {
            var services = new ServiceCollection();
            services.AddPwnedPasswordHttpClient();
            var provider = services.BuildServiceProvider();

            //all called in one method to easily enforce timout

            var service = new PwnedPasswordsClient(
                provider.GetService<IHttpClientFactory>().CreateClient(PwnedPasswordsClient.DefaultName),
                MockHelpers.StubLogger<PwnedPasswordsClient>());
            return service;
        }
    }
}
