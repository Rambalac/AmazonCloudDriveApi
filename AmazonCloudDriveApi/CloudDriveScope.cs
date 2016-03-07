// <copyright file="CloudDriveScope.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// App access scope
    /// </summary>
    [Flags]
    public enum CloudDriveScope
    {
        /// <summary>
        /// Read images
        /// </summary>
        ReadImage = 1,

        /// <summary>
        /// Read videos
        /// </summary>
        ReadVideo = 2,

        /// <summary>
        /// Read documents
        /// </summary>
        ReadDocument = 4,

        /// <summary>
        /// Read other non video, image or document files
        /// </summary>
        ReadOther = 8,

        /// <summary>
        /// Read all files
        /// </summary>
        ReadAll = 16,

        /// <summary>
        /// Upload files
        /// </summary>
        Write = 32
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
