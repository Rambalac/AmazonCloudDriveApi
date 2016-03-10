// <copyright file="HttpWebException.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Runtime.Serialization;

namespace Azi.Tools
{
    /// <summary>
    /// Exception with HTTP status code
    /// </summary>
    [Serializable]
    public class HttpWebException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebException"/> class.
        /// Creates exception with message and HTTP status code
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="code">Status code</param>
        public HttpWebException(string message, HttpStatusCode code)
            : base(message)
        {
            StatusCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebException"/> class.
        /// Creates exception with message, HTTP status code and inner exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="code">Status code</param>
        /// <param name="e">Inner exception</param>
        public HttpWebException(string message, HttpStatusCode code, Exception e)
            : base(message, e)
        {
            StatusCode = code;
        }

        /// <summary>
        /// Gets HTTP Status Code
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}