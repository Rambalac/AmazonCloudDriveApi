using System;

namespace Azi.Amazon.CloudDrive.JsonObjects
{

    /// <summary>
    /// Cloud drive auth tokens
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// Tokens creation time
        /// </summary>
        public DateTime createdTime = DateTime.UtcNow;

        /// <summary>
        /// True if expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > (createdTime + TimeSpan.FromSeconds(expires_in - 60));
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string token_type { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int expires_in { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string refresh_token { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string access_token { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}