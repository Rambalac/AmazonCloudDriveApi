// <copyright file="AuthToken.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Azi.Amazon.CloudDrive.JsonObjects
{
    /// <summary>
    /// Cloud drive auth tokens
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// Gets tokens creation time
        /// </summary>
        public DateTime createdTime { get; internal set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets a value indicating whether true if expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > (createdTime + TimeSpan.FromSeconds(expires_in - 60));

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public string refresh_token { get; set; }

        public string access_token { get; set; }
    }
}
#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
