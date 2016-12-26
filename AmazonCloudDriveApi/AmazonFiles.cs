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
        async Task<Stream> IAmazonFiles.Download(string id)
        {
            var content = await GetContentUrl().ConfigureAwait(false);
            var url = $"{content}nodes/{id}/content";
            return new DownloadStream(http, url);
        }

        /// <inheritdoc/>
        async Task IAmazonFiles.Download(string id, Stream stream, long? fileOffset, long? length, int bufferSize, Func<long, long> progress)
        {
            Func<long, Task<long>> progressAsync = null;
            if (progress != null)
            {
                progressAsync = p => Task.FromResult(progress(p));
            }

            await Files.Download(id, stream, fileOffset, length, bufferSize, progressAsync).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonFiles.Download(string id, Stream stream, long? fileOffset, long? length, int bufferSize, Func<long, Task<long>> progressAsync)
        {
            var content = await GetContentUrl().ConfigureAwait(false);
            var url = $"{content}nodes/{id}/content";

            await http.GetToStreamAsync(url, stream, fileOffset, length, bufferSize, progressAsync).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonFiles.Download(string id, Func<HttpWebResponse, Task> streammer, long? fileOffset, long? length)
        {
            var content = await GetContentUrl().ConfigureAwait(false);
            var url = $"{content}nodes/{id}/content";
            await http.GetToStreamAsync(url, streammer, fileOffset, length).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<int> IAmazonFiles.Download(string id, byte[] buffer, int bufferIndex, long fileOffset, int length)
        {
            var content = await GetContentUrl().ConfigureAwait(false);
            var url = $"{content}nodes/{id}/content";
            return await http.GetToBufferAsync(url, buffer, bufferIndex, fileOffset, length).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.Overwrite(string id, Func<Stream> streamCreator, CancellationToken? cancellation, Func<long, long> progress)
        {
            var content = await GetContentUrl().ConfigureAwait(false);
            var url = $"{content}nodes/{id}/content";
            var file = new SendFileInfo
            {
                StreamOpener = streamCreator,
                FileName = id,
                FormName = "content",
                CancellationToken = cancellation
            };
            if (progress != null)
            {
                file.Progress = p => Task.FromResult(progress(p));
            }

            return await http.SendFile<AmazonNode>(HttpMethod.Put, url, file).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.UploadNew(string parentId, string fileName, Func<Stream> streamOpener, bool allowDuplicate)
        {
            var fileUpload = new FileUpload
            {
                ParentId = parentId,
                AllowDuplicate = allowDuplicate,
                FileName = fileName,
                StreamOpener = streamOpener
            };
            return await ((IAmazonFiles)this).UploadNew(fileUpload).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonFiles.UploadNew(FileUpload fileUpload)
        {
            var content = await GetContentUrl().ConfigureAwait(false);
            var url = $"{content}nodes";
            if (fileUpload.AllowDuplicate)
            {
                url += "?suppress=deduplication";
            }

            var obj = new NewChild { name = fileUpload.FileName, parents = new[] { fileUpload.ParentId }, kind = "FILE" };
            var meta = JsonConvert.SerializeObject(obj);

            var file = new SendFileInfo
            {
                StreamOpener = fileUpload.StreamOpener,
                FileName = fileUpload.FileName,
                FormName = "content",
                CancellationToken = fileUpload.CancellationToken,
                BufferSize = fileUpload.BufferSize,
                Progress = fileUpload.ProgressAsync,
                Parameters = new Dictionary<string, string>
                    {
                        { "metadata", meta }
                    }
            };
            if (file.Progress == null && fileUpload.Progress != null)
            {
                file.Progress = p => Task.FromResult(fileUpload.Progress(p));
            }

            return await http.SendFile<AmazonNode>(HttpMethod.Post, url, file).ConfigureAwait(false);
        }
    }
}