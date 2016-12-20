using System;
using System.Threading.Tasks;
using Xunit;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonDriveTests
    {
        [Fact]
        public async Task AuthenticationByExternalBrowserTest()
        {
            var amazon = new AmazonDrive(AmazonSecret.ClientId, AmazonSecret.ClientSecret);

            var result = await amazon.AuthenticationByExternalBrowser(CloudDriveScopes.ReadAll | CloudDriveScopes.Write, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            Assert.True(result);
        }
    }
}