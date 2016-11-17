// <copyright file="AmazonFiles.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

namespace Azi.Amazon.CloudDrive
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Tools;

    /// <summary>
    /// Seakable Stream for file download
    /// </summary>
    internal class DownloadStream : Stream
    {
        private HttpClient http;
        private long lastposition;
        private long? length;
        private long position;
        private HttpWebResponse response;
        private Stream responseStream;
        private string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadStream"/> class.
        /// </summary>
        /// <param name="http">HttpClient to make requests</param>
        /// <param name="url">URL for download</param>
        internal DownloadStream(HttpClient http, string url)
        {
            this.http = http;
            this.url = url;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                if (length == null)
                {
                    throw new NotSupportedException("Length can be received only after first read");
                }

                return (long)length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (responseStream != null)
            {
                responseStream.Close();
                responseStream.Dispose();
                responseStream = null;
            }

            if (response != null)
            {
                response.Close();
                response.Dispose();
                response = null;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).Result;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int? result = null;
            await Retry.Do(
                HttpClient.RetryTimes,
                HttpClient.RetryDelay,
                async () =>
                {
                    try
                    {
                        if (position != lastposition || responseStream == null)
                        {
                            if (position != lastposition)
                            {
                                Close();
                            }

                            lastposition = position;

                            var client = await http.GetHttpClient(url).ConfigureAwait(false);
                            if (position != 0)
                            {
                                client.AddRange(position);
                            }

                            client.Method = "GET";

                            response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);
                            var lengthStr = response.GetResponseHeader("Content-Length");
                            long len;
                            if (long.TryParse(lengthStr, out len))
                            {
                                length = position + len;
                            }

                            responseStream = response.GetResponseStream();
                        }

                        result = await responseStream.ReadAsync(buffer, offset, count);

                        position += result.Value;
                        lastposition += result.Value;
                        return true;
                    }
                    catch (Exception)
                    {
                        Close();
                        throw;
                    }
                }, http.GeneralExceptionProcessor);
            if (result == null)
            {
                throw new NullReferenceException("Read result was not set");
            }

            return result.Value;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.End:
                    position = Length - offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}