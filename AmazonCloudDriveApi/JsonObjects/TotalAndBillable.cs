// <copyright file="TotalAndBillable.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    public class TotalAndBillable
    {
        public Amount total { get; set; }

        public Amount billable { get; set; }
    }
}
#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
