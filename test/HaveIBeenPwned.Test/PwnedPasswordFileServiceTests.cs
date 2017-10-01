using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HaveIBeenPwned.Test
{
    public class PwnedPasswordFileServiceTests
    {
        [Fact]
        public async Task ForPlainTextPassword_WhenFileContainsPwnedPassword_ReturnsTrue()
        {
            var environment = new HostingEnvironment
            {
                ContentRootFileProvider =
                    new PhysicalFileProvider(Directory.GetCurrentDirectory())
            };

            var service = new PwnedPasswordFileService(
                new Mock<ILogger<PwnedPasswordFileService>>().Object,
                new PwnedPasswordFileServiceOptions { Filenames = new[] {"pwned-passwords-update-2.txt", } },
                environment
                );

            var pwnedPassword = "password";

            var isPwned = await service.HasPasswordBeenPwned(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        [Fact]
        public async Task ForPlainTextPassword_WhenFileDoesNotContainPwnedPassword_ReturnsFalse()
        {
            var environment = new HostingEnvironment
            {
                ContentRootFileProvider =
                    new PhysicalFileProvider(Directory.GetCurrentDirectory())
            };

            var service = new PwnedPasswordFileService(
                new Mock<ILogger<PwnedPasswordFileService>>().Object,
                new PwnedPasswordFileServiceOptions { Filenames = new[] {"pwned-passwords-update-2.txt" } },
                environment
                );

            var safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwned(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");
        }

        [Fact]
        public async Task ForSha1Password_WhenFileContainsPwnedPassword_ReturnsTrue()
        {
            var environment = new HostingEnvironment
            {
                ContentRootFileProvider =
                    new PhysicalFileProvider(Directory.GetCurrentDirectory())
            };

            var service = new PwnedPasswordFileService(
                new Mock<ILogger<PwnedPasswordFileService>>().Object,
                new PwnedPasswordFileServiceOptions { Filenames = new[] { "pwned-passwords-update-2.txt", } },
                environment
                );

            var pwnedPassword = "password";
            var sha1Password = SHA1Util.SHA1HashStringForUTF8String(pwnedPassword);

            var isPwned = await service.HasSha1PasswordBeenPwned(sha1Password);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        [Fact]
        public async Task ForSha1Password_WhenFileDoesNotContainPwnedPassword_ReturnsFalse()
        {
            var environment = new HostingEnvironment
            {
                ContentRootFileProvider =
                    new PhysicalFileProvider(Directory.GetCurrentDirectory())
            };

            var service = new PwnedPasswordFileService(
                new Mock<ILogger<PwnedPasswordFileService>>().Object,
                new PwnedPasswordFileServiceOptions { Filenames = new[] { "pwned-passwords-update-2.txt" } },
                environment
                );

            var safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";
            var sha1Password = SHA1Util.SHA1HashStringForUTF8String(safePassword);

            var isPwned = await service.HasSha1PasswordBeenPwned(sha1Password);

            Assert.False(isPwned, "Checking for safe password should return false");
        }
    }
}
