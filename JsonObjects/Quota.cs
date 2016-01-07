using System;

namespace Azi.Amazon.CloudDrive.JsonObjects
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Quota
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public long quota { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime lastCalculated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public long available { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}