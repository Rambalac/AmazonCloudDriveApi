using System;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    /// <summary>
    /// Cloud drive node status
    /// </summary>
    public enum AmazonNodeStatus
    {
        AVAILABLE,
        TRASH,
        PURGED
    }

    /// <summary>
    /// Cloud drive node kind
    /// </summary>
    public enum AmazonNodeKind
    {
        FILE,
        ASSET,
        FOLDER
    }

    /// <summary>
    /// Video properties
    /// </summary>
    public class AmazonNodeVideo
    {
        /// <summary>
        /// Video height
        /// </summary>
        public int height { get; set; }

    /// <summary>
        /// Video width
        /// </summary>
        public int width { get; set; }
    }

    /// <summary>
    /// Image properties
    /// </summary>
    public class AmazonNodeImage
    {
        /// <summary>
        /// Image height
        /// </summary>
        public int height { get; set; }

    /// <summary>
        /// Image width
        /// </summary>
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
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
