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
    /// Http helper class to send REST api requests
    /// </summary>
    internal class HttpClient
    {
        Func<HttpWebRequest, Task> settingsSetter;
        const int retryTimes = 100;

        TimeSpan retryDelay(int time)
        {
            return TimeSpan.FromSeconds(1 << time);
        }

        /// <summary>
        /// Constructs new class with initializing callback
        /// </summary>
        /// <param name="settingsSetter"></param>
        public HttpClient(Func<HttpWebRequest, Task> settingsSetter)
        {
            this.settingsSetter = settingsSetter;
        }
        private async Task<HttpWebRequest> GetHttpClient(string url)
        {
            var result = (HttpWebRequest)WebRequest.Create(url);

            await settingsSetter(result).ConfigureAwait(false);
            return result;
        }

        static T SearchForException<T>(Exception ex, int depth = 3) where T : class
        {
            T res = null;
            var cur = ex;
            for (int i = 0; i < depth; i++)
            {
                res = cur as T;
                if (res != null) return res;
                cur = ex.InnerException;
                if (cur == null) return null;
            }
            return null;
        }

        static readonly HashSet<HttpStatusCode> retryCodes = new HashSet<HttpStatusCode> { HttpStatusCode.ProxyAuthenticationRequired };

        /// <summary>
        /// Return false to continue
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        static bool GeneralExceptionProcessor(Exception ex)
        {
            if (ex is TaskCanceledException) return false;

            var webex = SearchForException<WebException>(ex);
            if (webex != null)
            {

                var webresp = webex.Response as HttpWebResponse;
                if (webresp != null)
                {
                    if (retryCodes.Contains(webresp.StatusCode)) return false;
                    throw new HttpWebException(webex.Message, webresp.StatusCode);
                }
            }
            throw ex;
        }

        public async Task<T> GetJsonAsync<T>(string url)
        {
            return await Send<T>(HttpMethod.Get, url).ConfigureAwait(false);
        }

        public async Task GetToStreamAsync(string url, Stream stream, long? fileOffset = null, long? length = null, int bufferSize = 4096, Func<long, long> progress = null)
        {
            var start = DateTime.UtcNow;
            await GetToStreamAsync(url, async (response) =>
            {
                using (Stream input = response.GetResponseStream())
                {
                    byte[] buff = new byte[Math.Min(bufferSize, (response.ContentLength != -1) ? response.ContentLength : long.MaxValue)];
                    int red;
                    long nextProgress = -1;
                    while ((red = await input.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false)) > 0)
                    {
                        await stream.WriteAsync(buff, 0, red).ConfigureAwait(false);
                        if (progress != null && input.Position >= nextProgress)
                        {
                            nextProgress = progress.Invoke(input.Position);
                        }
                    }
                    if (nextProgress == -1) progress?.Invoke(0);
                }
            }, fileOffset, length).ConfigureAwait(false);
        }

        public async Task GetToStreamAsync(string url, Func<HttpWebResponse, Task> streammer, long? fileOffset = null, long? length = null)
        {
            await Retry.Do(retryTimes, retryDelay, async () =>
            {
                var client = await GetHttpClient(url).ConfigureAwait(false);
                if (fileOffset != null && length != null)
                    client.AddRange((long)fileOffset, (long)(fileOffset + length - 1));
                else
                    if (fileOffset != null && length == null)
                    client.AddRange((long)fileOffset);
                client.Method = "GET";

                using (var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode())
                    {
                        return await LogBadResponse(response).ConfigureAwait(false);
                    }

                    await streammer(response).ConfigureAwait(false);
                }
                return true;
            }, GeneralExceptionProcessor).ConfigureAwait(false);
        }


        public async Task<int> GetToBufferAsync(string url, byte[] buffer, int bufferIndex, long fileOffset, int length)
        {
            using (var stream = new MemoryStream(buffer, bufferIndex, length))
            {
                await GetToStreamAsync(url, stream, fileOffset, length).ConfigureAwait(false);
                return (int)stream.Position;
            }
        }

        public async Task<T> PostForm<T>(string url, Dictionary<string, string> pars)
        {
            return await SendForm<T>(HttpMethod.Post, url, pars).ConfigureAwait(false);
        }

        public async Task<R> Patch<P, R>(string url, P obj)
        {
            return await Send<P, R>(new HttpMethod("PATCH"), url, obj).ConfigureAwait(false);
        }

        public async Task<R> Post<P, R>(string url, P obj)
        {
            return await Send<P, R>(HttpMethod.Post, url, obj).ConfigureAwait(false);
        }

        public async Task<T> SendForm<T>(HttpMethod method, string url, Dictionary<string, string> pars)
        {
            T result = default(T);
            await Retry.Do(retryTimes, retryDelay, async () =>
            {
                var client = await GetHttpClient(url).ConfigureAwait(false);
                client.Method = method.ToString();
                using (var content = new FormUrlEncodedContent(pars))
                {
                    client.ContentType = content.Headers.ContentType.ToString();

                    using (var output = await client.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        await content.CopyToAsync(output).ConfigureAwait(false);
                    }
                }
                using (var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode())
                    {
                        return await LogBadResponse(response).ConfigureAwait(false);
                    }

                    result = await response.ReadAsAsync<T>().ConfigureAwait(false);
                }
                return true;
            }, GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        public async Task<R> Send<P, R>(HttpMethod method, string url, P obj)
        {
            return await Send(method, url, obj, (r) => r.ReadAsAsync<R>()).ConfigureAwait(false);
        }

        public async Task<R> Send<R>(HttpMethod method, string url)
        {
            return await Send(method, url, (r) => r.ReadAsAsync<R>()).ConfigureAwait(false);
        }

        public async Task<R> Send<P, R>(HttpMethod method, string url, P obj, Func<HttpWebResponse, Task<R>> responseParser)
        {
            R result = default(R);
            await Retry.Do(retryTimes, retryDelay, async () =>
            {
                var client = await GetHttpClient(url).ConfigureAwait(false);
                client.Method = method.ToString();
                var data = JsonConvert.SerializeObject(obj);
                using (var content = new StringContent(data))
                {
                    client.ContentType = content.Headers.ContentType.ToString();

                    using (var output = await client.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        await content.CopyToAsync(output).ConfigureAwait(false);
                    }
                }

                using (var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode())
                    {
                        return await LogBadResponse(response).ConfigureAwait(false);
                    }

                    result = await responseParser(response).ConfigureAwait(false);
                }
                return true;
            }, GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        public async Task<R> Send<R>(HttpMethod method, string url, Func<HttpWebResponse, Task<R>> responseParser)
        {
            R result = default(R);
            await Retry.Do(retryTimes, retryDelay, async () =>
            {
                var client = await GetHttpClient(url).ConfigureAwait(false);
                client.Method = method.ToString();

                using (var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode())
                    {
                        return await LogBadResponse(response).ConfigureAwait(false);
                    }

                    result = await responseParser(response).ConfigureAwait(false);
                }
                return true;
            }, GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        static Encoding UTF8 => new UTF8Encoding(false, true);

        private Stream GetMultipartFormPre(FileUpload file, long filelength, string boundry)
        {
            var result = new MemoryStream(1000);
            using (var writer = new StreamWriter(result, UTF8, 16, true))
            {
                if (file.Parameters != null)
                    foreach (var pair in file.Parameters)
                    {
                        writer.Write($"--{boundry}\r\n");
                        writer.Write($"Content-Disposition: form-data; name=\"{pair.Key}\"\r\n\r\n{pair.Value}\r\n");
                    }

                writer.Write($"--{boundry}\r\n");
                writer.Write($"Content-Disposition: form-data; name=\"{file.FormName}\"; filename={file.FileName}\r\n");
                writer.Write($"Content-Type: application/octet-stream\r\n");

                writer.Write($"Content-Length: {filelength}\r\n\r\n");
            }
            result.Position = 0;
            return result;
        }

        private Stream GetMultipartFormPost(FileUpload file, string boundry)
        {
            var result = new MemoryStream(1000);
            using (var writer = new StreamWriter(result, UTF8, 16, true))
            {
                writer.Write($"\r\n--{boundry}--\r\n");
            }
            result.Position = 0;
            return result;
        }

        public async Task<T> SendFile<T>(HttpMethod method, string url, FileUpload file)
        {
            T result = default(T);
            await Retry.Do(retryTimes, retryDelay, async () =>
            {
                var client = await GetHttpClient(url).ConfigureAwait(false);
                client.Method = method.ToString();
                client.AllowWriteStreamBuffering = false;

                string boundry = Guid.NewGuid().ToString();
                client.ContentType = $"multipart/form-data; boundary={boundry}";
                client.SendChunked = true;

                using (var input = file.StreamOpener())
                {

                    var pre = GetMultipartFormPre(file, input.Length, boundry);
                    var post = GetMultipartFormPost(file, boundry);
                    client.ContentLength = pre.Length + input.Length + post.Length;
                    using (var output = await client.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        await pre.CopyToAsync(output).ConfigureAwait(false);
                        await input.CopyToAsync(output).ConfigureAwait(false);
                        await post.CopyToAsync(output).ConfigureAwait(false);
                    }
                }
                using (var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode())
                    {
                        return await LogBadResponse(response).ConfigureAwait(false);
                    }

                    result = await response.ReadAsAsync<T>().ConfigureAwait(false);
                }
                return true;
            }, GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        private async Task PushFile(Stream input, Stream output, int timeout)
        {
            using (input)
            using (output)
            {
                var buf = new byte[81920];
                int red;
                do
                {
                    red = await input.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
                    if (red == 0) break;
                    using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout)))
                    {
                        await output.WriteAsync(buf, 0, red, cancellationSource.Token).ConfigureAwait(false);
                    }
                } while (red != 0);
            }
        }

        private async Task<bool> LogBadResponse(HttpWebResponse response)
        {
            try
            {
                var message = await response.ReadAsStringAsync().ConfigureAwait(false);
                if (!retryCodes.Contains(response.StatusCode)) throw new HttpWebException(message, response.StatusCode);
                return false;
            }
            catch (Exception e)
            {
                throw new HttpWebException(e.Message, response.StatusCode, e);
            }
        }
    }
}
