using System;
using System.Collections.Generic;

namespace Azi.Amazon.CloudDrive.JsonObjects
{
    /// <summary>
    /// Cloud drive node status
    /// </summary>
    public enum AmazonNodeStatus
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        AVAILABLE,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        TRASH,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        PURGED
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Cloud drive node kind
    /// </summary>
    public enum AmazonNodeKind
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        FILE,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        ASSET,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        FOLDER
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    public class AmazonNodeVideo
    {
        public int height { get; set; }
        public int width { get; set; }
    }

    public class AmazonNodeImage
    {
        public int height { get; set; }
        public int width { get; set; }
    }

    /// <summary>
    /// Cloud drive node information. See REST API
    /// </summary>
    public class AmazonNode
    {
        /// <summary>
        /// File size, 0 for folders.
        /// </summary>
        public long Length => contentProperties?.size ?? 0;

        /// <summary>
        /// Creation time
        /// </summary>
        public readonly DateTime FetchTime = DateTime.UtcNow;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string eTagResponse { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public AmazonNodeKind kind { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int version { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime modifiedDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime createdDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public IList<string> labels { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string createdBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public IList<string> parents { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public AmazonNodeStatus status { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool restricted { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ContentProperties contentProperties { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        public string tempLink { get; set; }

        public IList<AmazonNode> assets { get; set; }

        public AmazonNodeVideo video { get; set; }
        public AmazonNodeImage image { get; set; }
    }
}