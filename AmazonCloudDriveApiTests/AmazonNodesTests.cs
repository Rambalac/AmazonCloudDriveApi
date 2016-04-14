using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

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
            byte[] testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent));

            var node = await Amazon.Nodes.GetNodeExtended(testFile.id);
            Assert.NotNull(node.tempLink);
        }

        [Fact]
        public void GetChildrenTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Theory]
        [InlineData("test&file.txt")]
        [InlineData("test%file.txt")]
        [InlineData("t&.txt")]
        [InlineData("t%.txt")]

        public async Task GetChildTest(string name)
        {
            byte[] testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, name, () => new MemoryStream(testFileContent));

            await Task.Delay(1000);

            var node = await Amazon.Nodes.GetChild(TestDirId, name);

            Assert.Equal(testFile.id, node.id);
            Assert.Equal(name, node.name);
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