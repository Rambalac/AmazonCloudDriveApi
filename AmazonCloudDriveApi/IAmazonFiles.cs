// <copyright file="IAmazonFiles.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// File upload and download part of API
    /// </summary>
    public interface IAmazonFiles
    {
        /// <summary>
        /// Creates Stream to downloads file. Stream supports seeking reopening Http stream.
        /// </summary>
        /// <param name="id">File id to download.</param>
        /// <returns>Stream for file</returns>
        Task<Stream> Download(string id);

        /// <summary>
        /// Downloads file with low level response processor
        /// </summary>
        /// <param name="id">File id to download.</param>
        /// <param name="streammer">Async func called with HttpWebResponse</param>
        /// <param name="fileOffset">Offset in file to download from. By default is null to start from the beginning.</param>
        /// <param name="length">Length of part of file to download. By default is null to download everything to the end of file.</param>
        /// <returns>Async task</returns>
        Task Download(string id, Func<HttpWebResponse, Task> streammer, long? fileOffset = default(long?), long? length = default(long?));

        /// <summary>
        /// Downloads file to byte buffer
        /// </summary>
        /// <param name="id">File id to download.</param>
        /// <param name="buffer">Byte buffer for file</param>
        /// <param name="bufferIndex">Starting index in buffer to write data</param>
        /// <param name="fileOffset">Offset in file to download from. By default is null to start from the beginning.</param>
        /// <param name="length">Length of part of file to download. By default is null to download everything to the end of file.</param>
        /// <returns>Number of bytes read</returns>
        Task<int> Download(string id, byte[] buffer, int bufferIndex, long fileOffset, int length);

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
        Task Download(string id, Stream stream, long? fileOffset = default(long?), long? length = default(long?), int bufferSize = 4096, Func<long, long> progress = null);

        /// <summary>
        /// Overwrite file by id and stream
        /// </summary>
        /// <param name="id">File id to overwrite.</param>
        /// <param name="streamCreator">Func returning Stream for data. Can be called multiple times if retry happened. Stream will be closed by method.</param>
        /// <param name="token">Upload cancellation token</param>
        /// <param name="progress">Func called on progress with number of total downloaded bytes. Return next not exact boundary to call progress again.</param>
        /// <returns>Node info for overwritten file</returns>
        Task<AmazonNode> Overwrite(string id, Func<Stream> streamCreator, CancellationToken? token = null, Func<long, long> progress = null);

        /// <summary>
        /// Upload file to folder.
        /// </summary>
        /// <param name="parentId">Folder id for new file</param>
        /// <param name="fileName">Name of new file</param>
        /// <param name="streamCreator">Func returning Stream for data. Can be called multiple times if retry happened. Stream will be closed by method.</param>
        /// <param name="allowDuplicate">True to allow duplicate uploads.
        /// If it's False and file MD5 is the same as some other file in the cloud HTTP error Conflict will be thrown</param>
        /// <returns>Node info for new file</returns>
        Task<AmazonNode> UploadNew(string parentId, string fileName, Func<Stream> streamCreator, bool allowDuplicate = true);

        /// <summary>
        /// Upload file to folder.
        /// </summary>
        /// <param name="fileUpload">Information about new file</param>
        /// <returns>Node info for new file</returns>
        Task<AmazonNode> UploadNew(FileUpload fileUpload);
    }
}