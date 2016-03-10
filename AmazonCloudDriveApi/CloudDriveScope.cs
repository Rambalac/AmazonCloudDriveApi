// <copyright file="CloudDriveScope.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Application access scope
    /// </summary>
    [Flags]
    public enum CloudDriveScopes
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