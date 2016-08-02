using Xunit;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonProfileTests : AmazonTestsBase
    {
        [Fact]
        public async Task GetProfileTest()
        {
            var profile = await Amazon.Profile.GetProfile();
            Assert.NotNull(profile);
            Assert.NotNull(profile.user_id);
            Assert.NotNull(profile.email);
            Assert.NotNull(profile.name);
        }
    }
}