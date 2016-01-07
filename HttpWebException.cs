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
    [Serializable]
    public class HttpWebException : Exception
    {
        public readonly HttpStatusCode StatusCode;
        public HttpWebException(string message, HttpStatusCode code) : base(message)
        {
            this.StatusCode = code;
        }
        public HttpWebException(string message, HttpStatusCode code, Exception e) : base(message, e)
        {
            this.StatusCode = code;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}