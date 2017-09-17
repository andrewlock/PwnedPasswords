using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Test;
using Xunit;

namespace HaveIBeenPwnedValidator.Test
{
    public class PwnedPasswordApiServiceTests
    {
        [Fact, Trait("Category", "Integration")] // don't run it automatically
        public async Task WhenApiIsCalledReturnsCorrectValue()
        {
            //all called in one method to easily enforce timout

            var service = new PwnedPasswordApiService(
                MockHelpers.StubLogger<PwnedPasswordApiService>(),
                new PwnedPasswordApiServiceOptions()
                );

            var safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwned(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");

            var pwnedPassword = "password";

            isPwned = await service.HasPasswordBeenPwned(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }
    }
}
