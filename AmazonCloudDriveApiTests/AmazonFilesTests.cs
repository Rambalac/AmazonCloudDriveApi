using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonFilesTests : AmazonTestsBase
    {
        private const string testFileName = "testFile.txt";

        [Fact]
        public void OverwriteTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public async void UploadNewCancallationTest()
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
        public async void DownloadWithProgressCancelTest()
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
        public async void DownloadWithProgressTest()
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