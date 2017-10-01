using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HaveIBeenPwned.Test
{
    public class PwnedPasswordApiServiceTests
    {
        [Fact, Trait("Category", "Integration")] // don't run it automatically
        public async Task WhenApiIsCalledReturnsCorrectValue()
        {
            //all called in one method to easily enforce timout

            var service = new PwnedPasswordApiService(
                new Mock<ILogger<PwnedPasswordApiService>>().Object,
                new PwnedPasswordApiServiceOptions()
                //{
                //    ApiUrl = "http://localhost:5000/api/v2/pwnedpassword"
                //}
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
