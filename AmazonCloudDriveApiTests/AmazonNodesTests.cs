using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonNodesTests : AmazonTestsBase
    {


        [Fact]
        public async Task GetNodeTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));

            await Task.Delay(1000);

            var node = await Amazon.Nodes.GetNode(testFile.id);

            Assert.Equal(testFile.id, node.id);
            Assert.Equal("testfile.txt", node.name);
        }

        [Fact]
        public async Task GetNodeExtendedTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));

            var node = await Amazon.Nodes.GetNodeExtended(testFile.id);
            Assert.NotNull(node.tempLink);
        }

        [Fact]
        public async Task GetChildrenTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testSubDir = await Amazon.Nodes.CreateFolder(TestDirId, "TestDir");

            var testFile1 = await ForceRetry(async () => await Amazon.Files.UploadNew(testSubDir.id, "testfile1.txt", () => new MemoryStream(testFileContent)));
            var testFile2 = await ForceRetry(async () => await Amazon.Files.UploadNew(testSubDir.id, "testfile2.txt", () => new MemoryStream(testFileContent)));

            await Task.Delay(1000);

            var nodes = await Amazon.Nodes.GetChildren(testSubDir.id);

            Assert.True(nodes.Any(n => n.id == testFile1.id && n.name == "testfile1.txt"));
            Assert.True(nodes.Any(n => n.id == testFile2.id && n.name == "testfile2.txt"));
        }

        [Theory]
        [InlineData("test&file.txt")]
        [InlineData("test%file.txt")]
        [InlineData("t&.txt")]
        [InlineData("t%.txt")]
        [InlineData(@"t\.txt")]
        public async Task GetChildTest(string name)
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, name, () => new MemoryStream(testFileContent)));

            await Task.Delay(1000);

            var node = await Amazon.Nodes.GetChild(TestDirId, name);

            Assert.Equal(testFile.id, node.id);
            Assert.Equal(name, node.name);
        }

        [Fact]
        public async Task AddRemoveTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile1 = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));
            await Task.Delay(1000);

            var testSubDir = await Amazon.Nodes.CreateFolder(TestDirId, "TestDir");
            await Amazon.Nodes.Add(testSubDir.id, testFile1.id);
            await Task.Delay(1000);

            var nodes = await Amazon.Nodes.GetChildren(testSubDir.id);

            Assert.Equal(1, nodes.Count);
            Assert.True(nodes.Any(n => n.id == testFile1.id && n.name == "testfile.txt"));

            await Amazon.Nodes.Remove(testSubDir.id, testFile1.id);
            await Task.Delay(1000);

            var nodes2 = await Amazon.Nodes.GetChildren(testSubDir.id);
            Assert.Equal(0, nodes2.Count);

            var node = await Amazon.Nodes.GetChild(TestDirId, "testfile.txt");
            Assert.True(node.id == testFile1.id && node.name == "testfile.txt");
        }
        [Fact]
        public async Task TrashTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile1 = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));
            await Task.Delay(1000);

            var testSubDir = await Amazon.Nodes.CreateFolder(TestDirId, "TestDir");
            await Amazon.Nodes.Add(testSubDir.id, testFile1.id);
            await Task.Delay(1000);

            var nodes = await Amazon.Nodes.GetChildren(testSubDir.id);

            Assert.Equal(1, nodes.Count);
            Assert.True(nodes.Any(n => n.id == testFile1.id && n.name == "testfile.txt"));

            await Amazon.Nodes.Trash(testFile1.id);
            await Task.Delay(1000);

            var nodes2 = await Amazon.Nodes.GetChildren(testSubDir.id);
            Assert.Equal(0, nodes2.Count);

            var node = await Amazon.Nodes.GetChild(TestDirId, "testfile.txt");
            Assert.Null(node);
        }

        [Fact]
        public async Task GetRootTest()
        {
            await Task.Delay(1000);
            var root = await Amazon.Nodes.GetRoot();
            var nodes = await Amazon.Nodes.GetChildren(root.id);
            Assert.True(nodes.Any(n => n.id == TestDirId), $"Expected {TestDirId}, found {string.Join("|", nodes.Select(n => n.name))}");
        }

        [Fact]
        public async Task RenameTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));

            await Task.Delay(1000);

            var node = await Amazon.Nodes.GetNode(testFile.id);

            Assert.Equal(testFile.id, node.id);
            Assert.Equal("testfile.txt", node.name);

            await Amazon.Nodes.Rename(testFile.id, "newname.txt");

            var node2 = await Amazon.Nodes.GetNode(testFile.id);

            Assert.Equal(testFile.id, node2.id);
            Assert.Equal("newname.txt", node2.name);
        }


        [Fact]
        public async Task MoveTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile1 = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));
            await Task.Delay(1000);

            Assert.Equal(TestDirId, testFile1.parents.Single());

            var testSubDir = await Amazon.Nodes.CreateFolder(TestDirId, "TestDir");
            await Amazon.Nodes.Move(testFile1.id, TestDirId, testSubDir.id);

            var node = await Amazon.Nodes.GetNode(testFile1.id);

            Assert.Equal(TestDirId, testSubDir.parents.Single());
        }

        static string GetMd5Hash(byte[] input)
        {
            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(input);
                return string.Concat(data.Select(b => b.ToString("x2")));
            }
        }

        [Fact]
        public async Task GetNodesByMD5Test()
        {
            var rand = RandomNumberGenerator.Create();
            var testFileContent = new byte[1000];
            rand.GetBytes(testFileContent);
            var md5 = GetMd5Hash(testFileContent).ToUpperInvariant();

            var testFile = await ForceRetry(async () => await Amazon.Files.UploadNew(TestDirId, "testfile.txt", () => new MemoryStream(testFileContent)));

            await Task.Delay(1000);

            var nodes = await Amazon.Nodes.GetNodesByMD5(md5);
            Assert.Equal(1, nodes.Count);
            Assert.True(nodes.Any(n => n.id == testFile.id && n.name == "testfile.txt" && n.parents.Single() == TestDirId));
        }
    }
}