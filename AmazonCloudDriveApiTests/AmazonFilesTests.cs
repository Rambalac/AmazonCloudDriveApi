using System;
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
        private const string TestFileName = "testFile.txt";

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
            var testFileContent = Enumerable.Range(1, 100).Select(i => (byte)(i & 255)).ToArray();
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
            var testFile = await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent));
            Assert.Equal(0, testFile.Length);

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        [Fact]
        public async Task UploadNewCancallationTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var token = new CancellationTokenSource();
            token.Cancel();

            var fileUpload = new FileUpload
            {
                ParentId = TestDirId,
                FileName = TestFileName,
                StreamOpener = () => new MemoryStream(testFileContent),
                CancellationToken = token.Token
            };

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await Amazon.Files.UploadNew(fileUpload));
        }

        [Fact]
        public async Task UploadNewProgressTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var totalProgressCalls = 0;
            var fileUpload = new FileUpload
            {
                ParentId = TestDirId,
                FileName = TestFileName,
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
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent));

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
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent));

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr, progress: Progress1);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        [Fact]
        public async Task DownloadWithSeekableStreamTest()
        {
            var testFileContent = Enumerable.Range(0, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent));

            var stream=await Amazon.Files.Download(testFile.id);

            var testBuf = new byte[100];
            stream.Position = 100;
            await stream.ReadAsync(testBuf, 0, testBuf.Length);

            Assert.Equal(Enumerable.Range(100, 100).Select(i => (byte)(i & 255)).ToArray(), testBuf);

            testBuf = new byte[300];
            stream.Position = 500;
            await stream.ReadAsync(testBuf, 0, testBuf.Length);

            Assert.Equal(Enumerable.Range(500, 300).Select(i => (byte)(i & 255)).ToArray(), testBuf);

            testBuf = new byte[100];
            stream.Position = 50;
            await stream.ReadAsync(testBuf, 0, 50);
            await stream.ReadAsync(testBuf, 50, 50);

            Assert.Equal(Enumerable.Range(50, 100).Select(i => (byte)(i & 255)).ToArray(), testBuf);
        }

        private long Progress1(long arg)
        {
            return arg + 10;
        }
    }
}