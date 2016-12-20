using System.Threading.Tasks;
using Xunit;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonAccountTests : AmazonTestsBase
    {
        [Fact]
        public async Task GetEndpointTest()
        {
            await Amazon.Account.GetEndpoint();
        }

        [Fact]
        public async Task GetQuotaTest()
        {
            await Amazon.Account.GetQuota();
        }

        [Fact]
        public async Task GetUsageTest()
        {
            await Amazon.Account.GetUsage();
        }
    }
}