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
        /// <param name="accessToken">Authentication token</param>
        /// <param name="refreshToken">Authentication token refresh token</param>
        /// <param name="expiresIn">Authentication token expiration time</param>
        void OnTokenUpdated(string accessToken, string refreshToken, DateTime expiresIn);
    }
}
