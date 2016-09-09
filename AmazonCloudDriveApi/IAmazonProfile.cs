// <copyright file="IAmazonProfile.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Profile related part of API
    /// </summary>
    public interface IAmazonProfile
    {
        /// <summary>
        /// Request Amazon Profile info.
        /// </summary>
        /// <returns>Profile info</returns>
        Task<Profile> GetProfile();
    }
}