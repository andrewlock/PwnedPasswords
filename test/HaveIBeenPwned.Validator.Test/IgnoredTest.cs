using Xunit;

namespace HaveIBeenPwned.Validator.Test
{
    public class IgnoredTest
    {
        [Fact, Trait("Category", "Integration")] // don't run it automatically
        public void IgnoreMe()
        {
            // this test is necessary as the xunit filter requires that it has something to filter
        }
    }
}