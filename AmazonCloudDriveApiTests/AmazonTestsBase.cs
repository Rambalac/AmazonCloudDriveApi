using System;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive.Tests
{
    public abstract class AmazonTestsBase : ITokenUpdateListener, IDisposable
    {
        protected AmazonDrive Amazon;

        protected AmazonTestsBase()
        {
            Amazon = Authenticate().Result;
            var rootId = Amazon.Nodes.GetRoot().Result.id;
            var testDir = TestDirBase + new Random().Next();
            var node = Amazon.Nodes.GetChild(rootId, testDir).Result ??
                       Amazon.Nodes.CreateFolder(rootId, testDir).Result;
            TestDirId = node.id;
        }

        protected async Task<AmazonDrive> Authenticate()
        {
            var settings = Properties.Settings.Default;

            // AmazonSecret is in git ignore because Amazon App info should not be public. 
            // So to run tests you need to create your own class with your App Id and Secret.
            var amazon = new AmazonDrive(AmazonSecret.ClientId, AmazonSecret.ClientSecret) { OnTokenUpdate = this };

            if (!string.IsNullOrWhiteSpace(settings.AuthRenewToken))
            {
                if (await amazon.AuthenticationByTokens(
                    settings.AuthToken,
                    settings.AuthRenewToken,
                    settings.AuthTokenExpiration))
                {
                    return amazon;
                }
            }

            if (await amazon.AuthenticationByExternalBrowser(CloudDriveScopes.ReadAll | CloudDriveScopes.Write | CloudDriveScopes.Profile, TimeSpan.FromMinutes(10)))
            {
                return amazon;
            }

            return null;
        }

        protected const string TestDirBase = "ACDDokanNetTest";

        protected string TestDirId;

        public void OnTokenUpdated(string accessToken, string refreshToken, DateTime expiresIn)
        {
            var settings = Properties.Settings.Default;

            settings.AuthToken = accessToken;
            settings.AuthRenewToken = refreshToken;
            settings.AuthTokenExpiration = expiresIn;
            settings.Save();
        }

        public void Dispose()
        {
            if (Amazon == null || TestDirId == null) return;
            Amazon.Nodes.Trash(TestDirId).Wait();
        }
    }
}