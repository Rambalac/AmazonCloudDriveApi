using System;
using System.Threading.Tasks;
using Xunit;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonDriveTests
    {
        [Fact(Skip = "Not automatic")]
        public async Task AuthenticationByExternalBrowserTest()
        {
            var amazon = new AmazonDrive(AmazonSecret.ClientId, AmazonSecret.ClientSecret);

            var result = await amazon.AuthenticationByExternalBrowser(CloudDriveScopes.ReadAll | CloudDriveScopes.Write, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            Assert.True(result);
        }

        [Fact]
        public void AmazonDriveTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void SafeAuthenticationAsyncTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}