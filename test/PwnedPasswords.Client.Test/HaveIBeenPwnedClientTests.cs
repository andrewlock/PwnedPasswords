using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PwnedPasswords.Client.Test
{
    public class HaveIBeenPwnedClientTests
    {
        [Theory, Trait("Category", "Integration")] // don't run it automatically
        [InlineData(false), InlineData(true)]
        public async Task HasPasswordBeenPwned_WhenStrongPassword_ReturnsFalse(bool addPadding)
        {
            PwnedPasswordsClient service = GetClient(addPadding: addPadding);

            var safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwned(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");
        }

        [Theory, Trait("Category", "Integration")] // don't run it automatically
        [InlineData(false), InlineData(true)]
        public async Task HasPasswordBeenPwned_WhenWeakPassword_ReturnsTrue(bool addPadding)
        {
            PwnedPasswordsClient service = GetClient(addPadding: addPadding);

            var pwnedPassword = "password";

            var isPwned = await service.HasPasswordBeenPwned(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        [Theory, Trait("Category", "Integration")] // don't run it automatically
        [InlineData(false), InlineData(true)]
        public async Task HasPasswordBeenPwned_WhenWeakPasswordButUnderThresholdViews_ReturnsFalse(bool addPadding)
        {
            PwnedPasswordsClient service = GetClient(5000, addPadding: addPadding);

            var pwnedPassword = "Password1!";

            var isPwned = await service.HasPasswordBeenPwned(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        private static PwnedPasswordsClient GetClient(int minimumFrequencyToConsiderPwned = 1, bool addPadding = false)
        {
            var services = new ServiceCollection();
            services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned, addPadding);
            var provider = services.BuildServiceProvider();

            //all called in one method to easily enforce timout

            var service = new PwnedPasswordsClient(
                provider.GetService<IHttpClientFactory>().CreateClient(PwnedPasswordsClient.DefaultName),
                MockHelpers.StubLogger<PwnedPasswordsClient>(),
                MockHelpers.Options<PwnedPasswordsClientOptions>());
            return service;
        }
    }
}
