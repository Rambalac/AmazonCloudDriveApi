// <copyright file="ContentProperties.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    public class ContentProperties
    {
        public long size { get; set; }

        public int version { get; set; }

        public string contentType { get; set; }

        public string md5 { get; set; }

        public string extension { get; set; }

        public DateTime contentDate { get; set; }

        public Image image { get; set; }
    }
}
#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
