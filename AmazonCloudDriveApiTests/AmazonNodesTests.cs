// <copyright file="AmazonNodesTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Xunit;
using System;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonNodesTests : ITokenUpdateListener
    {
        protected async Task<AmazonDrive> Authenticate()
        {
            var settings = Properties.Settings.Default;
            var amazon = new AmazonDrive(AmazonSecret.clientId, AmazonSecret.clientSecret);
            amazon.OnTokenUpdate = this;

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

            if (await amazon.AuthenticationByExternalBrowser(CloudDriveScope.ReadAll | CloudDriveScope.Write, TimeSpan.FromMinutes(10)))
            {
                return amazon;
            }

            return null;
        }

        protected const string Testdir = "\\ACDDokanNetTest\\";

        [Fact]
        public void GetNodeTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public async Task GetNodeExtendedTest()
        {
            var amazon = await Authenticate();
            var node = await amazon.Nodes.GetNodeExtended("kqt2jeqTSnKQawvSXo3WiA");
        }

        [Fact]
        public void GetChildrenTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void GetChildTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void AddTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void RemoveTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void TrashTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void CreateFolderTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void GetRootTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void RenameTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void MoveTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void GetNodeByMD5Test()
        {
            Assert.True(false, "This test needs an implementation");
        }

        public void OnTokenUpdated(string access_token, string refresh_token, DateTime expires_in)
        {
            var settings = Properties.Settings.Default;

            settings.AuthToken = access_token;
            settings.AuthRenewToken = refresh_token;
            settings.AuthTokenExpiration = expires_in;
            settings.Save();
        }
    }
}