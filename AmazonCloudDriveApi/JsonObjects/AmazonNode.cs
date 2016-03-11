// <copyright file="AmazonNode.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    /// <summary>
    /// Cloud drive node status
    /// </summary>
    public enum AmazonNodeStatus
    {
        /// <summary>
        /// Amazon node AVAILABLE
        /// </summary>
        AVAILABLE,

        /// <summary>
        /// Amazon node TRASHed
        /// </summary>
        TRASH,

        /// <summary>
        /// Amazon node PURGED
        /// </summary>
        PURGED
    }

    /// <summary>
    /// Cloud drive node kind
    /// </summary>
    public enum AmazonNodeKind
    {
        /// <summary>
        /// File node
        /// </summary>
        FILE,

        /// <summary>
        /// Asset node
        /// </summary>
        ASSET,

        /// <summary>
        /// Folder node
        /// </summary>
        FOLDER
    }

    /// <summary>
    /// Cloud drive node information. See REST API
    /// </summary>
    public class AmazonNode
    {
        /// <summary>
        /// Gets creation time
        /// </summary>
        public DateTime FetchTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// Gets file size, 0 for folders.
        /// </summary>
        public long Length => contentProperties?.size ?? 0;

        public string eTagResponse { get; set; }

        public string id { get; set; }

        public string name { get; set; }

        public AmazonNodeKind kind { get; set; }

        public int version { get; set; }

        public DateTime modifiedDate { get; set; }

        public DateTime createdDate { get; set; }

        public IList<string> labels { get; set; }

        public string createdBy { get; set; }

        public IList<string> parents { get; set; }

        public AmazonNodeStatus status { get; set; }

        public bool restricted { get; set; }

        public ContentProperties contentProperties { get; set; }

        public string tempLink { get; set; }

        public IList<AmazonNode> assets { get; set; }

        public AmazonNodeVideo video { get; set; }

        public AmazonNodeImage image { get; set; }
    }

    public class AmazonBulkOperation : AmazonV2
    {
        public string op { get; set; }

        public List<string> value { get; set; }
    }

    public class AmazonSharedCollection
    {
        public string id { get; set; }

        public string shareURL { get; set; }

        public string shareId { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore SA1600 // Elements must be documented
