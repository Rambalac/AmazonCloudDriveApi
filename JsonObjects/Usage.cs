using System;

namespace Azi.Amazon.CloudDrive.JsonObjects
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Usage
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class TotalAndBillable
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public class Amount
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                public long bytes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                public long count { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public Amount total { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public Amount billable { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// Total size of all files of all types
        /// </summary>
        public TotalAndBillable total
        {
            get
            {
                return new TotalAndBillable
                {
                    total = new TotalAndBillable.Amount
                    {
                        bytes = other.total.bytes + doc.total.bytes + photo.total.bytes + video.total.bytes,
                        count = other.total.count + doc.total.count + photo.total.count + video.total.count
                    },
                    billable = new TotalAndBillable.Amount
                    {
                        bytes = other.billable.bytes + doc.billable.bytes + photo.billable.bytes + video.billable.bytes,
                        count = other.billable.count + doc.billable.count + photo.billable.count + video.billable.count
                    }
                };
            }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DateTime lastCalculated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TotalAndBillable other { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TotalAndBillable doc { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TotalAndBillable photo { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TotalAndBillable video { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}