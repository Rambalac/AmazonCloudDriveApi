// <copyright file="AmazonAccount.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Part to work with account info
    /// </summary>
    public partial class AmazonDrive
    {
        private static readonly TimeSpan EndpointExpiration = TimeSpan.FromDays(3);
        private Endpoint endpoint;
        private Quota quota;
        private Usage usage;

        /// <inheritdoc/>
        async Task<Endpoint> IAmazonAccount.GetEndpoint()
        {
            if (endpoint == null || DateTime.UtcNow - endpoint.lastCalculated > EndpointExpiration)
            {
                endpoint = await http.GetJsonAsync<Endpoint>("https://drive.amazonaws.com/drive/v1/account/endpoint").ConfigureAwait(false);
            }

            return endpoint;
        }

        /// <inheritdoc/>
        async Task<Quota> IAmazonAccount.GetQuota()
        {
            if (quota == null || DateTime.UtcNow - quota.lastCalculated > GeneralExpiration)
            {
                var metadataUrl = await GetMetadataUrl().ConfigureAwait(false);
                quota = await http.GetJsonAsync<Quota>(string.Format("{0}account/quota", metadataUrl)).ConfigureAwait(false);
            }

            return quota;
        }

        /// <inheritdoc/>
        async Task<Usage> IAmazonAccount.GetUsage()
        {
            if (usage == null || DateTime.UtcNow - usage.lastCalculated > GeneralExpiration)
            {
                var metadataUrl = await GetMetadataUrl().ConfigureAwait(false);
                usage = await http.GetJsonAsync<Usage>(string.Format("{0}account/usage", metadataUrl)).ConfigureAwait(false);
            }

            return usage;
        }
    }
}
