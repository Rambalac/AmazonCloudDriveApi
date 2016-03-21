// <copyright file="FileUpload.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Azi.Tools
{
    /// <summary>
    /// Information about file to upload
    /// </summary>
    internal class SendFileInfo
    {
        /// <summary>
        /// Gets or sets file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets form name
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Gets or sets multipart parameters
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Gets or sets Stream creator func with content to upload. Can be requested multiple time in case of retry.
        /// </summary>
        public Func<Stream> StreamOpener { get; set; }

        /// <summary>
        /// Gets or sets upload Cancellation Token
        /// </summary>
        public CancellationToken? CancellationToken { get; set; } = null;

        /// <summary>
        /// Gets or sets size of memory buffer for stream operations
        /// </summary>
        public int BufferSize { get; set; } = 81920;

        /// <summary>
        /// Gets or sets Func that receive progress and provide next position for progress report.
        /// Next position is not guarantied and depends on buffer size.
        /// </summary>
        public Func<long, long> Progress { get; set; } = null;
    }
}