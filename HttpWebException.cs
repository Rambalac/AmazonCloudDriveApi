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
    /// <summary>
    /// Http error with Status Code
    /// </summary>
    [Serializable]
    public class HttpWebException : Exception
    {
        /// <summary>
        /// Http Status Code
        /// </summary>
        public readonly HttpStatusCode StatusCode;
        internal HttpWebException(string message, HttpStatusCode code) : base(message)
        {
            this.StatusCode = code;
        }
        internal HttpWebException(string message, HttpStatusCode code, Exception e) : base(message, e)
        {
            this.StatusCode = code;
        }
    }
}