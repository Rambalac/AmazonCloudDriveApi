// <copyright file="Image.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    public class Image
    {
        public string make { get; set; }

        public string model { get; set; }

        public string exposureTime { get; set; }

        public DateTime dateTimeOriginal { get; set; }

        public string flash { get; set; }

        public string focalLength { get; set; }

        public DateTime dateTime { get; set; }

        public DateTime dateTimeDigitized { get; set; }

        public string software { get; set; }

        public string orientation { get; set; }

        public string colorSpace { get; set; }

        public string meteringMode { get; set; }

        public string exposureProgram { get; set; }

        public string exposureMode { get; set; }

        public string whiteBalance { get; set; }

        public string sensingMethod { get; set; }

        public string xResolution { get; set; }

        public string yResolution { get; set; }

        public string resolutionUnit { get; set; }

        public int width { get; set; }

        public int height { get; set; }
    }
}
#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
