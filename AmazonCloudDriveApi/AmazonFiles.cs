// <copyright file="AmazonFiles.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;
using Newtonsoft.Json;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Part to work with file upload and download
    /// </summary>
    public partial class AmazonDrive
    {
        /// <inheritdoc/>
        async Task IAmazonFiles.Download(string id, Stream stream, long? fileOffset, long? length, int bufferSize, Func<long, long> progress)
        {
            var url = string.Format("{0}nodes/{1}/content", await GetContentUrl().ConfigureAwait(false), id);
            await http.GetToStreamAsync(url, stream, fileOffset, length, bufferSize, progress).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonFiles.Download(string id, Func<HttpWebResponse, Task> streammer, long? fileOffset, long? length)
        {
            var url = string.Format("{0}nodes/{1}/content", await GetContentUrl().ConfigureAwait(false), id);
            await http.GetToStreamAsync(url, streammer, fileOffset, length).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<int> IAmazonFiles.Download(string id, byte[] buffer, int bufferIndex, long fileOffset, int length)
        {
            var url = string.Format("{0}nodes/{1}/content", await GetContentUrl().ConfigureAwait(false), id);
            return await http.GetToBufferAsync(url, buffer, bufferIndex, fileOffset, length).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.Overwrite(string id, Func<Stream> streamCreator)
        {
            var url = string.Format("{0}nodes/{1}/content", await GetContentUrl().ConfigureAwait(false), id);
            var file = new FileUpload
            {
                StreamOpener = streamCreator,
                FileName = id,
                FormName = "content"
            };
            return await http.SendFile<AmazonNode>(HttpMethod.Put, url, file).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.UploadNew(string parentId, string fileName, Func<Stream> streamCreator, bool allowDuplicate)
        {
            var url = string.Format("{0}nodes", await GetContentUrl().ConfigureAwait(false));
            if (allowDuplicate)
            {
                url += "?suppress=deduplication";
            }

            var obj = new NewChild { name = fileName, parents = new string[] { parentId }, kind = "FILE" };
            string meta = JsonConvert.SerializeObject(obj);

            var file = new FileUpload
            {
                StreamOpener = streamCreator,
                FileName = fileName,
                FormName = "content",
                Parameters = new Dictionary<string, string>
                    {
                        { "metadata", meta }
                    }
            };
            return await http.SendFile<AmazonNode>(HttpMethod.Post, url, file).ConfigureAwait(false);
        }
    }
}