using System;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Azi.Amazon.CloudDrive.Tests
{
    public abstract class AmazonTestsBase : ITokenUpdateListener, IDisposable
    {
        protected AmazonDrive Amazon;

        protected AmazonTestsBase()
        {
            Task.Run(async () =>
            {
                Amazon = await Authenticate();
                var rootId = (await Amazon.Nodes.GetRoot()).id;
                var testDir = TestDirBase + new Random().Next();
                var node = await Amazon.Nodes.GetChild(rootId, testDir) ??
                           await Amazon.Nodes.CreateFolder(rootId, testDir);
                TestDirId = node.id;
            }).Wait();
        }

        protected async Task<AmazonNode> ForceRetry(Func<Task<AmazonNode>> action)
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    return await action();
                }
                catch (HttpWebException ex) when ((int)ex.StatusCode == 429)
                {
                    await Task.Delay(1000);
                }
            }
            throw new Exception("Retied too much");
        }

        protected async Task<AmazonNode> ForceUploadNew(FileUpload fu)
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    return await Amazon.Files.UploadNew(fu);
                }
                catch (HttpWebException ex) when ((int)ex.StatusCode == 429)
                {
                    await Task.Delay(1000);
                }
            }
            throw new Exception("Retied too much");
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