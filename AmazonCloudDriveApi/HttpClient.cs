// <copyright file="HttpClient.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Azi.Tools
{
    /// <summary>
    /// Http helper class to send REST API requests
    /// </summary>
    internal class HttpClient
    {
        /// <summary>
        /// Maximum number of retries.
        /// </summary>
        public const int RetryTimes = 100;

        private static readonly HashSet<HttpStatusCode> RetryCodes = new HashSet<HttpStatusCode> { HttpStatusCode.ProxyAuthenticationRequired };
        private readonly Dictionary<int, Func<HttpStatusCode, Task<bool>>> retryErrorProcessor = new Dictionary<int, Func<HttpStatusCode, Task<bool>>>();
        private readonly Func<HttpWebRequest, Task> settingsSetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClient"/> class.
        /// Constructs new class with initializing callback.
        /// </summary>
        /// <param name="settingsSetter">Async func to configure HttpWebRequest</param>
        public HttpClient(Func<HttpWebRequest, Task> settingsSetter)
        {
            this.settingsSetter = settingsSetter;
        }

        private static Encoding UTF8 => new UTF8Encoding(false, true);

        /// <summary>
        /// Returns delay interval depended on number of retries.
        /// </summary>
        /// <param name="time">Number of retry</param>
        /// <returns>Time for delay</returns>
        public static TimeSpan RetryDelay(int time)
        {
            return TimeSpan.FromSeconds(1 << time);
        }

        /// <summary>
        /// Add Http error processor
        /// </summary>
        /// <param name="code">Http status</param>
        /// <param name="func">func to return true if request should be retried. Func reference will be stored as WeakReference, so be careful with anonymous func.
        /// Do not use method reference, use lamda or lamda with method call istead</param>
        public void AddRetryErrorProcessor(HttpStatusCode code, Func<HttpStatusCode, Task<bool>> func)
        {
            retryErrorProcessor[(int)code] = func;
        }

        /// <summary>
        /// Add Http error processor
        /// </summary>
        /// <param name="code">Http status code</param>
        /// <param name="func">func to return true if request should be retried. Func reference will be stored as WeakReference, so be careful with anonymous func.
        /// Do not use method reference, use lamda or lamda with method call istead</param>
        public void AddRetryErrorProcessor(int code, Func<HttpStatusCode, Task<bool>> func)
        {
            retryErrorProcessor[code] = func;
        }

        /// <summary>
        /// Processes exception to decide retry or abort.
        /// </summary>
        /// <param name="ex">Exception to process</param>
        /// <returns>False if retry</returns>
        public async Task<bool> GeneralExceptionProcessor(Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                throw ex;
            }

            var webex = SearchForException<WebException>(ex);
            if (webex != null)
            {
                var webresp = webex.Response as HttpWebResponse;
                if (webresp != null)
                {
                    if (RetryCodes.Contains(webresp.StatusCode))
                    {
                        return false;
                    }

                    Func<HttpStatusCode, Task<bool>> func;
                    if (retryErrorProcessor.TryGetValue((int)webresp.StatusCode, out func))
                    {
                        if (func != null)
                        {
                            if (await func(webresp.StatusCode).ConfigureAwait(false))
                            {
                                return false;
                            }
                        }
                    }

                    throw new HttpWebException(webex.Message, webresp.StatusCode);
                }
            }

            throw ex;
        }

        /// <summary>
        /// Returns configured raw HttpWebRequest
        /// </summary>
        /// <param name="url">Request URL</param>
        /// <returns>HttpWebRequest</returns>
        public async Task<HttpWebRequest> GetHttpClient(string url)
        {
            var result = (HttpWebRequest)WebRequest.Create(url);

            await settingsSetter(result).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Sends GET request and parses response as JSON
        /// </summary>
        /// <typeparam name="T">type or response</typeparam>
        /// <param name="url">URL for request</param>
        /// <returns>parsed response</returns>
        public async Task<T> GetJsonAsync<T>(string url)
        {
            return await Send<T>(HttpMethod.Get, url).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends GET request and put response to byte array
        /// </summary>
        /// <param name="url">URL for request</param>
        /// <param name="buffer">Byte array to get data into</param>
        /// <param name="bufferIndex">Offset in buffer</param>
        /// <param name="fileOffset">Offset in file</param>
        /// <param name="length">Number of bytes to download</param>
        /// <returns>Number of bytes downloaded</returns>
        public async Task<int> GetToBufferAsync(string url, byte[] buffer, int bufferIndex, long fileOffset, int length)
        {
            using (var stream = new MemoryStream(buffer, bufferIndex, length))
            {
                await GetToStreamAsync(url, stream, fileOffset, length).ConfigureAwait(false);
                return (int)stream.Position;
            }
        }

        /// <summary>
        /// Sends GET request and put response to Stream
        /// </summary>
        /// <param name="url">URL for request</param>
        /// <param name="stream">Stream to push result</param>
        /// <param name="fileOffset">Offset in file</param>
        /// <param name="length">Number of bytes to download</param>
        /// <param name="bufferSize">Size of memory buffer for download</param>
        /// <param name="progress">Func for progress. Parameter is current progress, result should be next position after which progress func will be called again</param>
        /// <returns>Async Task</returns>
        public async Task GetToStreamAsync(string url, Stream stream, long? fileOffset = null, long? length = null, int bufferSize = 4096, Func<long, long> progress = null)
        {
            var start = DateTime.UtcNow;
            await GetToStreamAsync(
                url,
                async (response) =>
                    {
                        using (Stream input = response.GetResponseStream())
                        {
                            byte[] buff = new byte[Math.Min(bufferSize, (response.ContentLength != -1) ? response.ContentLength : long.MaxValue)];
                            int red;
                            long nextProgress = -1;
                            long totalRead = 0;
                            while ((red = await input.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false)) > 0)
                            {
                                totalRead += red;
                                await stream.WriteAsync(buff, 0, red).ConfigureAwait(false);
                                if (progress != null && totalRead >= nextProgress)
                                {
                                    nextProgress = progress.Invoke(totalRead);
                                }
                            }
                            if (nextProgress == -1)
                            {
                                progress?.Invoke(0);
                            }
                        }
                    },
                fileOffset,
                length).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends GET request and put response to Stream
        /// </summary>
        /// <param name="url">URL for request</param>
        /// <param name="streammer">Async Func to process response</param>
        /// <param name="fileOffset">Offset in file</param>
        /// <param name="length">Number of bytes to download</param>
        /// <returns>Async Task</returns>
        public async Task GetToStreamAsync(string url, Func<HttpWebResponse, Task> streammer, long? fileOffset = null, long? length = null)
        {
            await Retry.Do(
                RetryTimes,
                RetryDelay,
                async () =>
                    {
                        var client = await GetHttpClient(url).ConfigureAwait(false);
                        if (fileOffset != null && length != null)
                        {
                            client.AddRange((long)fileOffset, (long)(fileOffset + length - 1));
                        }
                        else if (fileOffset != null && length == null)
                        {
                            client.AddRange((long)fileOffset);
                        }

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
                    },
                GeneralExceptionProcessor).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends PATCH request with object serialized to JSON and get response as parsed JSON
        /// </summary>
        /// <typeparam name="P">Request object type</typeparam>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="url">URL for request</param>
        /// <param name="obj">Object for request</param>
        /// <returns>Async result object</returns>
        public async Task<R> Patch<P, R>(string url, P obj)
        {
            return await Send<P, R>(new HttpMethod("PATCH"), url, obj).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends POST request with object serialized to JSON and get response as parsed JSON
        /// </summary>
        /// <typeparam name="P">Request object type</typeparam>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="url">URL for request</param>
        /// <param name="obj">Object for request</param>
        /// <returns>Async result object</returns>
        public async Task<R> Post<P, R>(string url, P obj)
        {
            return await Send<P, R>(HttpMethod.Post, url, obj).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends POST request with parameters
        /// </summary>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="url">URL for request</param>
        /// <param name="pars">Post parameters</param>
        /// <returns>Async result object</returns>
        public async Task<R> PostForm<R>(string url, Dictionary<string, string> pars)
        {
            return await SendForm<R>(HttpMethod.Post, url, pars).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes Http error processor
        /// </summary>
        /// <param name="code">Http status</param>
        public void RemoveRetryErrorProcessor(HttpStatusCode code)
        {
            retryErrorProcessor.Remove((int)code);
        }

        /// <summary>
        /// Removes Http error processor
        /// </summary>
        /// <param name="code">Http status code</param>
        public void RemoveRetryErrorProcessor(int code)
        {
            retryErrorProcessor.Remove(code);
        }

        /// <summary>
        /// Sends request with object serialized to JSON and get response as parsed JSON
        /// </summary>
        /// <typeparam name="P">Request object type</typeparam>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="method">Http method</param>
        /// <param name="url">URL for request</param>
        /// <param name="obj">Object for request</param>
        /// <returns>Async result object</returns>
        public async Task<R> Send<P, R>(HttpMethod method, string url, P obj)
        {
            return await Send(method, url, obj, (r) => r.ReadAsAsync<R>()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends request and get response as parsed JSON
        /// </summary>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="url">URL for request</param>
        /// <returns>Async result object</returns>
        public async Task<R> Send<R>(HttpMethod method, string url)
        {
            return await Send(method, url, (r) => r.ReadAsAsync<R>()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends request with object serialized to JSON and get response as parsed JSON
        /// </summary>
        /// <typeparam name="P">Request object type</typeparam>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="url">URL for request</param>
        /// <param name="obj">Object for request</param>
        /// <param name="responseParser">Func to parse response and return result object</param>
        /// <returns>Async result object</returns>
        public async Task<R> Send<P, R>(HttpMethod method, string url, P obj, Func<HttpWebResponse, Task<R>> responseParser)
        {
            R result = default(R);
            await Retry.Do(
                RetryTimes,
                RetryDelay,
                async () =>
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
                    },
                GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Sends request and get response as parsed JSON
        /// </summary>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="url">URL for request</param>
        /// <param name="responseParser">Func to parse response and return result object</param>
        /// <returns>Async result object</returns>
        public async Task<R> Send<R>(HttpMethod method, string url, Func<HttpWebResponse, Task<R>> responseParser)
        {
            R result = default(R);
            await Retry.Do(
                RetryTimes,
                RetryDelay,
                async () =>
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
                    },
                GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Uploads file
        /// </summary>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="url">URL for request</param>
        /// <param name="file">File upload parameters. Input stream must support Length</param>
        /// <returns>Async result object</returns>
        public async Task<R> SendFile<R>(HttpMethod method, string url, SendFileInfo file)
        {
            R result = default(R);
            await Retry.Do(
                RetryTimes,
                RetryDelay,
                async () =>
                    {
                        var client = await GetHttpClient(url).ConfigureAwait(false);
                        try
                        {
                            client.Method = method.ToString();
                            client.AllowWriteStreamBuffering = false;

                            var boundry = Guid.NewGuid().ToString();
                            client.ContentType = $"multipart/form-data; boundary={boundry}";
                            client.SendChunked = true;

                            using (var input = file.StreamOpener())
                            {
                                var pre = GetMultipartFormPre(file, input.Length, boundry);
                                var post = GetMultipartFormPost(boundry);
                                client.ContentLength = pre.Length + input.Length + post.Length;
                                using (var output = await client.GetRequestStreamAsync().ConfigureAwait(false))
                                {
                                    var state = new CopyStreamState();
                                    await CopyStreams(pre, output, file, state).ConfigureAwait(false);
                                    await CopyStreams(input, output, file, state).ConfigureAwait(false);
                                    await CopyStreams(post, output, file, state).ConfigureAwait(false);
                                }
                            }
                            using (var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false))
                            {
                                if (!response.IsSuccessStatusCode())
                                {
                                    return await LogBadResponse(response).ConfigureAwait(false);
                                }

                                result = await response.ReadAsAsync<R>().ConfigureAwait(false);
                            }
                            return true;
                        }
                        catch (Exception)
                        {
                            client.Abort();
                            throw;
                        }
                    },
                GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Sends request with form data
        /// </summary>
        /// <typeparam name="R">Result type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="url">URL for request</param>
        /// <param name="pars">Request parameters</param>
        /// <returns>Async result object</returns>
        public async Task<R> SendForm<R>(HttpMethod method, string url, Dictionary<string, string> pars)
        {
            R result = default(R);
            await Retry.Do(
                RetryTimes,
                RetryDelay,
                async () =>
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

                            result = await response.ReadAsAsync<R>().ConfigureAwait(false);
                        }
                        return true;
                    },
                GeneralExceptionProcessor).ConfigureAwait(false);
            return result;
        }

        private static async Task CopyStreams(Stream source, Stream destination, SendFileInfo info, CopyStreamState state)
        {
            var buffer = new byte[info.BufferSize];
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                if (info.CancellationToken != null && info.CancellationToken.Value.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                state.Pos += bytesRead;
                if (info.Progress != null && state.Pos >= state.NextPos)
                {
                    state.NextPos = info.Progress.Invoke(state.Pos);
                }
            }
        }

        private static Stream GetMultipartFormPost(string boundry)
        {
            var result = new MemoryStream(1000);
            using (var writer = new StreamWriter(result, UTF8, 16, true))
            {
                writer.Write($"\r\n--{boundry}--\r\n");
            }

            result.Position = 0;
            return result;
        }

        private static Stream GetMultipartFormPre(SendFileInfo file, long filelength, string boundry)
        {
            var result = new MemoryStream(1000);
            using (var writer = new StreamWriter(result, UTF8, 16, true))
            {
                if (file.Parameters != null)
                {
                    foreach (var pair in file.Parameters)
                    {
                        writer.Write($"--{boundry}\r\n");
                        writer.Write($"Content-Disposition: form-data; name=\"{pair.Key}\"\r\n\r\n{pair.Value}\r\n");
                    }
                }

                writer.Write($"--{boundry}\r\n");
                writer.Write($"Content-Disposition: form-data; name=\"{file.FormName}\"; filename={file.FileName}\r\n");
                writer.Write($"Content-Type: application/octet-stream\r\n");

                writer.Write($"Content-Length: {filelength}\r\n\r\n");
            }

            result.Position = 0;
            return result;
        }

        private static async Task<bool> LogBadResponse(HttpWebResponse response)
        {
            try
            {
                var message = await response.ReadAsStringAsync().ConfigureAwait(false);
                if (!RetryCodes.Contains(response.StatusCode))
                {
                    throw new HttpWebException(message, response.StatusCode);
                }

                return false;
            }
            catch (Exception e)
            {
                throw new HttpWebException(e.Message, response.StatusCode, e);
            }
        }

        private static T SearchForException<T>(Exception ex, int depth = 3)
                                                    where T : class
        {
            T res = null;
            var cur = ex;
            for (int i = 0; i < depth; i++)
            {
                res = cur as T;
                if (res != null)
                {
                    return res;
                }

                cur = ex.InnerException;
                if (cur == null)
                {
                    return null;
                }
            }

            return null;
        }

        private class CopyStreamState
        {
            public long NextPos { get; set; } = -1;

            public long Pos { get; set; } = 0;
        }
    }
}
