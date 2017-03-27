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
        public async Task OverwriteTest()
        {
            var testName = "testfile.txt";
            var testFileContent1 = Enumerable.Range(1, 100).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async ()=>await Amazon.Files.UploadNew(TestDirId, testName, () => new MemoryStream(testFileContent1)));
            Assert.Equal(testName, testFile.name);

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr);

            Assert.Equal(testFileContent1, memStr.ToArray());

            var testFileContent2 = Enumerable.Range(1, 50).Select(i => (byte)(i & 255)).ToArray();
            var testFile2 = await Amazon.Files.Overwrite(testFile.id, () => new MemoryStream(testFileContent2));
            Assert.Equal(testName, testFile2.name);

            var memStr2 = new MemoryStream();
            await Amazon.Files.Download(testFile2.id, memStr2);

            Assert.Equal(testFileContent2, memStr2.ToArray());
        }

        [Theory]
        [InlineData("test&file.txt")]
        [InlineData("test%file.txt")]
        [InlineData("t&.txt")]
        [InlineData("t%.txt")]
        public async Task UploadNameTest(string testName)
        {
            var testFileContent = Enumerable.Range(1, 100).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async ()=>await Amazon.Files.UploadNew(TestDirId, testName, () => new MemoryStream(testFileContent)));
            Assert.Equal(testName, testFile.name);

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        [Fact]
        public async Task UploadZeroLengthTest()
        {
            var testFileContent = new byte[0];
            var testFile = await ForceRetry(async ()=>await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent)));
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

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await ForceRetry(async ()=>await Amazon.Files.UploadNew(fileUpload)));
        }

        [Fact]
        public async Task UploadNewProgressTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var totalProgressCalls = 0;
            long lastPos = 0;
            var fileUpload = new FileUpload
            {
                ParentId = TestDirId,
                FileName = TestFileName,
                StreamOpener = () =>
                {
                    totalProgressCalls = 0;
                    return new MemoryStream(testFileContent);
                },

                BufferSize = 10,
                Progress = (pos) =>
                {
                    output.WriteLine(pos.ToString());
                    totalProgressCalls++;
                    lastPos = pos;
                    return pos + 600;
                }
            };

            var node = await ForceRetry(async ()=>await Amazon.Files.UploadNew(fileUpload));
            Assert.NotNull(node);
            Assert.Equal(3, totalProgressCalls);
            Assert.Equal(testFileContent.Length, lastPos);
        }

        [Fact]
        public async Task UploadNewProgressAsyncTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var totalProgressCalls = 0;
            var fileUpload = new FileUpload
            {
                ParentId = TestDirId,
                FileName = TestFileName,
                StreamOpener = () =>
                {
                    totalProgressCalls = 0;
                    return new MemoryStream(testFileContent);
                },
                BufferSize = 10,
                ProgressAsync = async pos =>
                {
                    output.WriteLine(pos.ToString());
                    totalProgressCalls++;
                    await Task.Delay(1);
                    return pos + 10;
                }
            };

            var node = await ForceRetry(async ()=>await Amazon.Files.UploadNew(fileUpload));
            Assert.NotNull(node);
            Assert.Equal(100, totalProgressCalls); 
        }

        [Fact]
        public async Task DownloadWithProgressCancelTest()
        {
            var testFileContent = Enumerable.Range(1, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async ()=>await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent)));

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
            var testFile = await ForceRetry(async ()=>await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent)));

            var memStr = new MemoryStream();
            await Amazon.Files.Download(testFile.id, memStr, progress: Progress1);

            Assert.Equal(testFileContent, memStr.ToArray());
        }

        [Fact]
        public async Task DownloadWithSeekableStreamTest()
        {
            var testFileContent = Enumerable.Range(0, 1000).Select(i => (byte)(i & 255)).ToArray();
            var testFile = await ForceRetry(async ()=>await Amazon.Files.UploadNew(TestDirId, TestFileName, () => new MemoryStream(testFileContent)));

            var stream = await Amazon.Files.Download(testFile.id);

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

        private static long Progress1(long arg)
        {
            return arg + 10;
        }
    }
}