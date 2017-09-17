using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace HaveIBeenPwnedValidator.Test
{
    public class PwnedPasswordFileServiceTests
    {
        [Fact]
        public async Task WhenFileContainsPwnedPasswordReturnsTrue()
        {
            var environment = new HostingEnvironment
            {
                ContentRootFileProvider =
                    new PhysicalFileProvider(Directory.GetCurrentDirectory())
            };

            var service = new PwnedPasswordFileService(
                MockHelpers.StubLogger<PwnedPasswordFileService>(),
                new PwnedPasswordFileServiceOptions { Filenames = new[] { "pwned-passwords-update-2.txt" } },
                environment
                );

            var pwnedPassword = "password";

            var isPwned = await service.HasPasswordBeenPwned(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        [Fact]
        public async Task WhenFileDoesNotContainPwnedPasswordReturnsFalse()
        {
            var environment = new HostingEnvironment
            {
                ContentRootFileProvider =
                    new PhysicalFileProvider(Directory.GetCurrentDirectory())
            };

            var service = new PwnedPasswordFileService(
                MockHelpers.StubLogger<PwnedPasswordFileService>(),
                new PwnedPasswordFileServiceOptions { Filenames = new[] { "pwned-passwords-update-2.txt" } },
                environment
                );

            var safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwned(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");
        }
    }
}
