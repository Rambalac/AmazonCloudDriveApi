// <copyright file="Usage.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    public class Usage
    {
        /// <summary>
        /// Gets total size of all files of all types
        /// </summary>
        public TotalAndBillable total
        {
            get
            {
                return new TotalAndBillable
                {
                    total = new Amount
                    {
                        bytes = other.total.bytes + doc.total.bytes + photo.total.bytes + video.total.bytes,
                        count = other.total.count + doc.total.count + photo.total.count + video.total.count
                    },
                    billable = new Amount
                    {
                        bytes = other.billable.bytes + doc.billable.bytes + photo.billable.bytes + video.billable.bytes,
                        count = other.billable.count + doc.billable.count + photo.billable.count + video.billable.count
                    }
                };
            }
        }

        public DateTime lastCalculated { get; set; }

        public TotalAndBillable other { get; set; }

        public TotalAndBillable doc { get; set; }

        public TotalAndBillable photo { get; set; }

        public TotalAndBillable video { get; set; }
    }
}
#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
