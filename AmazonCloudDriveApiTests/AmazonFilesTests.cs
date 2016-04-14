using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonFilesTests : AmazonTestsBase
    {
        private const string testFileName = "testFile.txt";

        private readonly ITestOutputHelper output;

        public AmazonFilesTests(ITestOutputHelper testOutputHelper)
        {
            output = testOutputHelper;
        }


        [Fact]
        public void OverwriteTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Theory]
        [InlineData("test&file.txt")]
        [InlineData("test%file.txt")]
        [InlineData("t&.txt")]
        [InlineData("t%.txt")]
        public async Task UploadNameTest(string testName)
        {
            byte[] testFileContent = Enumerable.Range(1, 100).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, testName, () => new MemoryStream(testFileContent));
            Assert.Equal(testName, testFile.name);

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        [Fact(Skip ="API does not support zero length")]
        public async Task UploadZeroLengthTest()
        {
            var testFileContent = new byte[0];
            var testFile = await Amazon.Files.UploadNew(TestDirId, testFileName, () => new MemoryStream(testFileContent));
            Assert.Equal(0, testFile.Length);

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        [Fact]
        public async Task UploadNewCancallationTest()
        {
            byte[] testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var token = new CancellationTokenSource();
            token.Cancel();

            var fileUpload = new FileUpload
            {
                ParentId = TestDirId,
                FileName = testFileName,
                StreamOpener = () => new MemoryStream(testFileContent),
                CancellationToken = token.Token
            };

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await Amazon.Files.UploadNew(fileUpload));
        }

        [Fact]
        public async Task UploadNewProgressTest()
        {
            byte[] testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            int totalProgressCalls = 0;
            var fileUpload = new FileUpload
            {
                ParentId = TestDirId,
                FileName = testFileName,
                StreamOpener = () => new MemoryStream(testFileContent),
                BufferSize = 10,
                Progress = (pos) =>
                {
                    output.WriteLine(pos.ToString());
                    totalProgressCalls++;
                    return pos + 10;
                }
            };

            var node = await Amazon.Files.UploadNew(fileUpload);
            Assert.NotNull(node);
            Assert.Equal(138, totalProgressCalls); //Not only content, MIME headers are counted too
        }

        [Fact]
        public async Task DownloadWithProgressCancelTest()
        {
            byte[] testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, testFileName, () => new MemoryStream(testFileContent));

            var memStr = new MemoryStream();
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await Amazon.Files.Download(testFile.id, memStr, bufferSize: 10, progress: ProgressCancel));
            Assert.Equal(10, memStr.Position);
        }

        private long ProgressCancel(long arg)
        {
            throw new OperationCanceledException();
        }

        [Fact]
        public async Task DownloadWithProgressTest()
        {
            byte[] testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, testFileName, () => new MemoryStream(testFileContent));

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr, progress: Progress1);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        private long Progress1(long arg)
        {
            return arg + 10;
        }

        [Fact]
        public void DownloadTest1()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void DownloadTest2()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}