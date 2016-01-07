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
    public class FileUpload
    {
        public Func<Stream> StreamOpener;
        public Dictionary<string, string> Parameters;
        public string FormName;
        public string FileName;

        public int Timeout = 30000;
    }
}