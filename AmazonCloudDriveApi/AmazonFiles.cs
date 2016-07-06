// <copyright file="AmazonFiles.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

namespace Azi.Amazon.CloudDrive
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using JsonObjects;
    using Newtonsoft.Json;
    using Tools;

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
        async Task<AmazonNode> IAmazonFiles.Overwrite(string id, Func<Stream> streamCreator, CancellationToken? token, Func<long, long> progress)
        {
            var url = string.Format("{0}nodes/{1}/content", await GetContentUrl().ConfigureAwait(false), id);
            var file = new SendFileInfo
            {
                StreamOpener = streamCreator,
                FileName = id,
                FormName = "content",
                CancellationToken = token,
                Progress = progress
            };
            return await http.SendFile<AmazonNode>(HttpMethod.Put, url, file).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.UploadNew(string parentId, string fileName, Func<Stream> streamOpener, bool allowDuplicate)
        {
            return await ((IAmazonFiles)this).UploadNew(new FileUpload
            {
                ParentId = parentId,
                AllowDuplicate = allowDuplicate,
                FileName = fileName,
                StreamOpener = streamOpener
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.UploadNew(FileUpload fileUpload)
        {
            var url = string.Format("{0}nodes", await GetContentUrl().ConfigureAwait(false));
            if (fileUpload.AllowDuplicate)
            {
                url += "?suppress=deduplication";
            }

            var obj = new NewChild { name = fileUpload.FileName, parents = new string[] { fileUpload.ParentId }, kind = "FILE" };
            var meta = JsonConvert.SerializeObject(obj);

            var file = new SendFileInfo
            {
                StreamOpener = fileUpload.StreamOpener,
                FileName = fileUpload.FileName,
                FormName = "content",
                CancellationToken = fileUpload.CancellationToken,
                BufferSize = fileUpload.BufferSize,
                Progress = fileUpload.Progress,
                Parameters = new Dictionary<string, string>
                    {
                        { "metadata", meta }
                    }
            };
            return await http.SendFile<AmazonNode>(HttpMethod.Post, url, file).ConfigureAwait(false);
        }
    }
}