// <copyright file="ITokenUpdateListener.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Listener for Authentication Token updates
    /// </summary>
    public interface ITokenUpdateListener
    {
        /// <summary>
        /// Called when Authentication Token updated
        /// </summary>
        /// <param name="access_token">Authentication token</param>
        /// <param name="refresh_token">Authentication token refresh token</param>
        /// <param name="expires_in">Authentication token expiration time</param>
        void OnTokenUpdated(string access_token, string refresh_token, DateTime expires_in);
    }
}
