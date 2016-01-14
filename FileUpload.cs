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
    /// Information about file to upload
    /// </summary>
    internal class FileUpload
    {
        /// <summary>
        /// Returns Stream with content to upload. Can be requested multiple time in case of retry.
        /// </summary>
        public Func<Stream> StreamOpener;

        public Dictionary<string, string> Parameters;
        public string FormName;
        public string FileName;

        public int Timeout = 30000;
    }
}