using System;
using System.Threading.Tasks;
using Azi.Tools;
using Azi.Amazon.CloudDrive.JsonObjects;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Account related part of API
    /// </summary>
    public class AmazonAccount
    {
        Endpoint _endpoint;
        Quota _quota;
        Usage _usage;

        static readonly TimeSpan endpointExpiration = TimeSpan.FromDays(3);
        private readonly AmazonDrive amazon;
        private HttpClient http => amazon.http;
        private static TimeSpan generalExpiration => AmazonDrive.generalExpiration;

        internal AmazonAccount(AmazonDrive amazonDrive)
        {
            this.amazon = amazonDrive;
        }

        /// <summary>
        /// Request for API endpoints. Cached in memory for 3 days.
        /// </summary>
        /// <returns></returns>
        public async Task<Endpoint> GetEndpoint()
        {
            if (_endpoint == null || DateTime.UtcNow - _endpoint.lastCalculated > endpointExpiration)
            {
                _endpoint = await http.GetJsonAsync<Endpoint>("https://drive.amazonaws.com/drive/v1/account/endpoint").ConfigureAwait(false);
            }
            return _endpoint;
        }

        /// <summary>
        /// Request for drive quota info.
        /// </summary>
        /// <returns></returns>
        public async Task<Quota> GetQuota()
        {
            if (_quota == null || DateTime.UtcNow - _quota.lastCalculated > generalExpiration)
            {
                var metadataUrl = await amazon.GetMetadataUrl().ConfigureAwait(false);
                _quota = await http.GetJsonAsync<Quota>(string.Format("{0}account/quota", metadataUrl)).ConfigureAwait(false);
            }
            return _quota;
        }

        /// <summary>
        /// Request for drive usage info.
        /// </summary>
        /// <returns></returns>
        public async Task<Usage> GetUsage()
        {
            if (_usage == null || DateTime.UtcNow - _usage.lastCalculated > generalExpiration)
            {
                var metadataUrl = await amazon.GetMetadataUrl().ConfigureAwait(false);
                _usage = await http.GetJsonAsync<Usage>(string.Format("{0}account/usage", metadataUrl)).ConfigureAwait(false);
            }
            return _usage;
        }
    }
}
