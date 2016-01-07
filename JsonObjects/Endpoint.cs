using System;

namespace Azi.Amazon.CloudDrive.JsonObjects
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Endpoint
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public readonly DateTime lastCalculated = DateTime.UtcNow;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool customerExists { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string contentUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string metadataUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
