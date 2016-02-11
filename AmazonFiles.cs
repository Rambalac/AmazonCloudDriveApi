using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClient = Azi.Tools.HttpClient;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// File upload and download part of API
    /// </summary>
    public class AmazonFiles
    {
        private readonly AmazonDrive amazon;
        private HttpClient http => amazon.http;

        internal AmazonFiles(AmazonDrive amazonDrive)
        {
            amazon = amazonDrive;
        }


        /// <summary>
        /// Overwrite file by id and stream
        /// </summary>
        /// <param name="id">File id to overwrite.</param>
        /// <param name="streamCreator">Func returning Stream for data. Can be called multiple times if retry happened. Stream will be closed by method.</param>
        /// <returns>Node info for overwritten file</returns>
        public async Task<AmazonNode> Overwrite(string id, Func<Stream> streamCreator)
        {
            var url = string.Format("{0}nodes/{1}/content", await amazon.GetContentUrl().ConfigureAwait(false), id);
            var file = new FileUpload
            {
                StreamOpener = streamCreator,
                FileName = id,
                FormName = "content"
            };
            return await http.SendFile<AmazonNode>(HttpMethod.Put, url, file).ConfigureAwait(false);
        }

        /// <summary>
        /// Upload file to folder.
        /// </summary>
        /// <param name="parentId">Folder id for new file</param>
        /// <param name="fileName">Name of new file</param>
        /// <param name="streamCreator">Func returning Stream for data. Can be called multiple times if retry happened. Stream will be closed by method.</param>
        /// <param name="allowDuplicate">True to allow duplicate uploads. 
        /// If it's False and file MD5 is the same as some other file in the cloud HTTP error Conflict will be thrown</param>
        /// <returns>Node info for new file</returns>
        public async Task<AmazonNode> UploadNew(string parentId, string fileName, Func<Stream> streamCreator, bool allowDuplicate=true)
        {
            var url = string.Format("{0}nodes", await amazon.GetContentUrl().ConfigureAwait(false));
            if (allowDuplicate) url += "?suppress=deduplication";

            var obj = new NewChild { name = fileName, parents = new string[] { parentId }, kind = "FILE" };
            string meta = JsonConvert.SerializeObject(obj);

            var file = new FileUpload
            {
                StreamOpener = streamCreator,
                FileName = fileName,
                FormName = "content",
                Parameters = new Dictionary<string, string>
                    {
                        {"metadata", meta}
                    }
            };
            return await http.SendFile<AmazonNode>(HttpMethod.Post, url, file).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads file to stream
        /// </summary>
        /// <param name="id">File id to download.</param>
        /// <param name="stream">Stream to write file data into.</param>
        /// <param name="fileOffset">Offset in file to download from. By default is null to start from the beginning.</param>
        /// <param name="length">Length of part of file to download. By default is null to download everything to the end of file.</param>
        /// <param name="bufferSize">Size of memory buffer. 4096 bytes by default.</param>
        /// <param name="progress">Func called on progress with number of total downloaded bytes. Return next not exact boundary to call progress again.</param>
        /// <returns>Async task</returns>
        public async Task Download(string id, Stream stream, long? fileOffset = null, long? length = null, int bufferSize = 4096, Func<long, long> progress = null)
        {
            var url = string.Format("{0}nodes/{1}/content", await amazon.GetContentUrl().ConfigureAwait(false), id);
            await http.GetToStreamAsync(url, stream, fileOffset, length, bufferSize, progress).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads file with low level responce processor
        /// </summary>
        /// <param name="id">File id to download.</param>
        /// <param name="streammer">Async func called with HttpWebResponse</param>
        /// <param name="fileOffset">Offset in file to download from. By default is null to start from the beginning.</param>
        /// <param name="length">Length of part of file to download. By default is null to download everything to the end of file.</param>
        /// <returns>Async task</returns>
        public async Task Download(string id, Func<HttpWebResponse, Task> streammer, long? fileOffset = null, long? length = null)
        {
            var url = string.Format("{0}nodes/{1}/content", await amazon.GetContentUrl().ConfigureAwait(false), id);
            await http.GetToStreamAsync(url, streammer, fileOffset, length).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads file to byte buffer
        /// </summary>
        /// <param name="id">File id to download.</param>
        /// <param name="buffer">Byte buffere for file</param>
        /// <param name="bufferIndex">Starting index in buffer to write data</param>
        /// <param name="fileOffset">Offset in file to download from. By default is null to start from the beginning.</param>
        /// <param name="length">Length of part of file to download. By default is null to download everything to the end of file.</param>
        /// <returns>Number of bytes read</returns>
        public async Task<int> Download(string id, byte[] buffer, int bufferIndex, long fileOffset, int length)
        {
            var url = string.Format("{0}nodes/{1}/content", await amazon.GetContentUrl().ConfigureAwait(false), id);
            return await http.GetToBufferAsync(url, buffer, bufferIndex, fileOffset, length).ConfigureAwait(false);
        }

    }
}