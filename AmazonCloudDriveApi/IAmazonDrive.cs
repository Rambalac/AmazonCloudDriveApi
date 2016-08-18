// <copyright file="IAmazonDrive.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Authentication and root interface to get other parts of API
    /// </summary>
    public interface IAmazonDrive
    {
        /// <summary>
        /// Gets account related part of API
        /// </summary>
        IAmazonAccount Account { get; }

        /// <summary>
        /// Gets file upload and download part of API
        /// </summary>
        IAmazonFiles Files { get; }

        /// <summary>
        /// Gets profile details of current user
        /// </summary>
        IAmazonProfile Profile { get; }

        /// <summary>
        /// Gets or sets start of 3 ports range used in localhost redirect listener for authentication with default port selector
        /// </summary>
        int ListenerPortStart { get; set; }

        /// <summary>
        /// Gets nodes management part of API
        /// </summary>
        IAmazonNodes Nodes { get; }

        /// <summary>
        /// Gets or sets byte array with response for redirection from authentication page
        /// </summary>
        byte[] CloseTabResponse { get; set; }

        /// <summary>
        /// Sets callback called when authentication token get updated on authentication or renewal. Using WeakReference
        /// </summary>
        ITokenUpdateListener OnTokenUpdate { set; }

        /// <summary>
        /// Authenticate using know Authentication token, renew token and expiration time
        /// </summary>
        /// <param name="authToken">Authentication token</param>
        /// <param name="authRenewToken">Authentication token renew token</param>
        /// <param name="authTokenExpiration">Authentication token expiration time</param>
        /// <returns>True if authenticated</returns>
        Task<bool> AuthenticationByTokens(string authToken, string authRenewToken, DateTime authTokenExpiration);

        /// <summary>
        /// Constructs URL for authentication
        /// </summary>
        /// <param name="redirectUrl">URL to redirect to after authentication. Must be registered in Amazon Developers Console.</param>
        /// <param name="scope">Access scope. Must be subset of application registered scopes in Amazon Developers Console.</param>
        /// <returns>URL string</returns>
        string BuildLoginUrl(string redirectUrl, CloudDriveScopes scope);

        /// <summary>
        /// Opens Amazon Cloud Drive authentication in default browser. Then it starts listener for port 45674 if portSelector is null
        /// </summary>
        /// <param name="scope">Your Application scope to access cloud</param>
        /// <param name="timeout">How long lister will wait for redirect before throw TimeoutException</param>
        /// <param name="cancelToken">Cancellation for authentication. Can be null.</param>
        /// <param name="unformatedRedirectUrl">URL to redirect to after authentication. To intercept redirect you need to redirect to local PC address. Use {0} for port substitute. Must be registered with exact ports in Amazon Developers Console.</param>
        /// <param name="portSelector">Func to select port for redirect listener.
        /// portSelector(int lastPort, int time) where lastPost is port used last time and
        /// time is number of times selector was called before. To abort port selection throw exception other than HttpListenerException</param>
        /// <returns>True if authenticated</returns>
        /// <exception cref="InvalidOperationException">if any selected port could not be opened by default selector</exception>
        Task<bool> AuthenticationByExternalBrowser(CloudDriveScopes scope, TimeSpan timeout, CancellationToken? cancelToken = default(CancellationToken?), string unformatedRedirectUrl = "http://localhost:{0}/signin/", Func<int, int, int> portSelector = null);

        /// <summary>
        /// Authenticate using authentication code
        /// </summary>
        /// <param name="code">Code from authentication redirect</param>
        /// <param name="redirectUrl">Redirect URL used to get authentication code</param>
        /// <returns>True if authenticated</returns>
        Task<bool> AuthenticationByCode(string code, string redirectUrl);
    }
}