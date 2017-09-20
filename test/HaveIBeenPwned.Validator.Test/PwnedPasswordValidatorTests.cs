using System;
using System.Threading.Tasks;
using HaveIBeenPwned;
using HaveIBeenPwnedValidator;
using Microsoft.AspNetCore.Identity.Test;
using Moq;
using Xunit;

namespace CommonPasswordsValidator.Test
{
    public class PwnedPasswordValidatorTests
    {
        const string _error = "The password you chose has appeared in a data breach.";

        [Fact]
        public void ValidateThrowsWithNullServiceTest()
        {
            // Setup
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>("service", () => new PwnedPasswordValidator<TestUser>(null));
        }

        [Fact]
        public async Task FailsIfNullPassword()
        {
            var service = new Mock<IPwnedPasswordService>();

            string input = null;
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(service.Object);

            IdentityResultAssert.IsFailure(await validator.ValidateAsync(manager, null, input), _error);
        }

        [Fact]
        public async Task FailsIfZeroLengthPassword()
        {
            var service = new Mock<IPwnedPasswordService>();

            var input = string.Empty;
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(service.Object);

            IdentityResultAssert.IsFailure(await validator.ValidateAsync(manager, null, input), _error);
        }

        [Fact]
        public async Task FailsIfServiceIndicatesPasswordIsPwned()
        {
            var service = new Mock<IPwnedPasswordService>();
            service.Setup(x => x.HasPasswordBeenPwned(It.IsAny<string>())).ReturnsAsync(true);

            var input = "password";
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(service.Object);

            IdentityResultAssert.IsFailure(await validator.ValidateAsync(manager, null, input), _error);
        }
        
        [Fact]
        public async Task SuccessIfServiceIndicatesPasswordIsNotPwned()
        {
            var service = new Mock<IPwnedPasswordService>();
            service.Setup(x => x.HasPasswordBeenPwned(It.IsAny<string>())).ReturnsAsync(false);

            var input = "password";
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(service.Object);

            IdentityResultAssert.IsSuccess(await validator.ValidateAsync(manager, null, input));
        }
        
    }
}