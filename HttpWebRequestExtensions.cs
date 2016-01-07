using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Azi.Tools
{
    public static class HttpWebRequestExtensions
    {
        static readonly HttpStatusCode[] successStatusCodes = { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.PartialContent };
        public static bool IsSuccessStatusCode(this HttpWebResponse response) => successStatusCodes.Contains(response.StatusCode);
        public static async Task<string> ReadAsStringAsync(this HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<T> ReadAsAsync<T>(this HttpWebResponse response)
        {
            var text = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static ContentRangeHeaderValue GetContentRange(this WebHeaderCollection headers)
        {
            return ContentRangeHeaderValue.Parse(headers["Content-Range"]);
        }
    }
}