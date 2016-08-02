// <copyright file="AmazonProfile.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Part to work with profile info
    /// </summary>
    public partial class AmazonDrive
    {
        private const string ProfileUrl = "https://api.amazon.com/user/profile";

        /// <inheritdoc/>
        async Task<Profile> IAmazonProfile.GetProfile()
        {
            var profile = await http.GetJsonAsync<Profile>(ProfileUrl).ConfigureAwait(false);

            return profile;
        }
    }
}