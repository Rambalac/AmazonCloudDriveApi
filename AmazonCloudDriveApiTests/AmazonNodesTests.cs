using Xunit;
using System;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonNodesTests : AmazonTestsBase
    {


        [Fact]
        public void GetNodeTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public async Task GetNodeExtendedTest()
        {
            var node = await Amazon.Nodes.GetNodeExtended("kqt2jeqTSnKQawvSXo3WiA");
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
    }
}