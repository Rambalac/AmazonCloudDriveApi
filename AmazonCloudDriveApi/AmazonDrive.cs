// <copyright file="AmazonDrive.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Root class for Amazon Cloud Drive API
    /// </summary>
    public sealed partial class AmazonDrive : IAmazonAccount, IAmazonFiles, IAmazonNodes, IAmazonDrive, IAmazonProfile
    {
        private const string LoginUrlBase = "https://www.amazon.com/ap/oa";
        private const string TokenUrl = "https://api.amazon.com/auth/o2/token";
        private const string DefaultOpenAuthResponse = "<SCRIPT>var win=window.open('{0}', '_blank');var id=setInterval(function(){{if (win.closed||win.location.href.indexOf('localhost')>=0){{clearInterval(id);win.close(); window.close();}}}}, 500);</SCRIPT>Please, allow popups if they got blocked";

        private static readonly TimeSpan GeneralExpiration = TimeSpan.FromMinutes(5);

        private static readonly Dictionary<CloudDriveScopes, string> ScopeToStringMap = new Dictionary<CloudDriveScopes, string>
        {
            { CloudDriveScopes.ReadImage, "clouddrive:read_image" },
            { CloudDriveScopes.ReadVideo, "clouddrive:read_video" },
            { CloudDriveScopes.ReadDocument, "clouddrive:read_document" },
            { CloudDriveScopes.ReadOther, "clouddrive:read_other" },
            { CloudDriveScopes.ReadAll, "clouddrive:read_all" },
            { CloudDriveScopes.Write, "clouddrive:write" },
            { CloudDriveScopes.Profile, "profile" },
            { CloudDriveScopes.Profile_UserIdOnly, "profile:user_id" },
            { CloudDriveScopes.Profile_PostalCode, "postal_code" },
        };

        private static readonly byte[] DefaultCloseTabResponse = Encoding.UTF8.GetBytes("<SCRIPT>window.close;</SCRIPT>You can close this tab");

        private static readonly RequestCachePolicy StandartCache = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

        private readonly HttpClient http;

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly SemaphoreSlim tokenUpdateSem = new SemaphoreSlim(1, 1);

        private AuthToken authTokens;

        private WeakReference<ITokenUpdateListener> weakOnTokenUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonDrive"/> class.
        /// </summary>
        /// <param name="clientId">Your Application ClientID. From Amazon Developers Console.</param>
        /// <param name="clientSecret">Your Application Secret. From Amazon Developers Console.</param>
        public AmazonDrive(string clientId, string clientSecret)
        {
            this.clientSecret = clientSecret;
            this.clientId = clientId;
            http = new HttpClient(SettingsSetter);
            http.AddRetryErrorProcessor(HttpStatusCode.Unauthorized, async (code) =>
            {
                await UpdateToken().ConfigureAwait(false);
                return true;
            });
            http.AddRetryErrorProcessor(429, async (code) =>
            {
                await Task.Delay(1000).ConfigureAwait(false);
                return true;
            });
            http.AddRetryErrorProcessor(500, (code) => Task.FromResult(true));
            http.AddRetryErrorProcessor(504, (code) => Task.FromResult(true));
            http.AddRetryErrorProcessor(HttpStatusCode.NotFound, (code) => Task.FromResult(false));
        }

        /// <inheritdoc/>
        public int ListenerPortStart { get; set; } = 45674;

        /// <inheritdoc/>
        public IAmazonAccount Account => this;

        /// <inheritdoc/>
        public IAmazonFiles Files => this;

        /// <inheritdoc/>
        public IAmazonNodes Nodes => this;

        /// <inheritdoc/>
        public IAmazonProfile Profile => this;

        /// <inheritdoc/>
        public ITokenUpdateListener OnTokenUpdate
        {
            set
            {
                weakOnTokenUpdate = new WeakReference<ITokenUpdateListener>(value);
            }
        }

        /// <inheritdoc/>
        public byte[] CloseTabResponse { get; set; } = DefaultCloseTabResponse;

        /// <inheritdoc/>
        public IWebProxy Proxy { get; set; }

        /// <inheritdoc/>
        public async Task<bool> AuthenticationByTokens(string authToken, string authRenewToken, DateTime authTokenExpiration)
        {
            authTokens = new AuthToken
            {
                expires_in = 0,
                createdTime = authTokenExpiration,
                access_token = authToken,
                refresh_token = authRenewToken,
                token_type = "bearer"
            };
            await UpdateToken().ConfigureAwait(false);
            return authTokens != null;
        }

        /// <inheritdoc/>
        public async Task<bool> AuthenticationByCode(string code, string redirectUrl)
        {
            var form = new Dictionary<string, string>
                                {
                                    { "grant_type", "authorization_code" },
                                    { "code", code },
                                    { "client_id", clientId },
                                    { "client_secret", clientSecret },
                                    { "redirect_uri", redirectUrl }
                                };
            authTokens = await http.PostForm<AuthToken>(TokenUrl, form).ConfigureAwait(false);
            if (authTokens == null)
            {
                return false;
            }

            CallOnTokenUpdate(authTokens.access_token, authTokens.refresh_token, DateTime.UtcNow.AddSeconds(authTokens.expires_in));

            await Account.GetEndpoint().ConfigureAwait(false);

            return true;
        }

        /// <inheritdoc/>
        public string BuildLoginUrl(string redirectUrl, CloudDriveScopes scope)
        {
            Contract.Assert(redirectUrl != null);

            return $"{LoginUrlBase}?client_id={clientId}&scope={ScopeToString(scope)}&response_type=code&redirect_uri={redirectUrl}";
        }

        /// <inheritdoc/>
        public async Task<bool> AuthenticationByExternalBrowser(CloudDriveScopes scope, TimeSpan timeout, CancellationToken? cancelToken = null, string unformatedRedirectUrl = "http://localhost:{0}/signin/", Func<int, int, int> portSelector = null)
        {
            string redirectUrl;
            using (var redirectListener = CreateListener(unformatedRedirectUrl, out redirectUrl, portSelector))
            {
                redirectListener.Start();
                var loginurl = BuildLoginUrl(redirectUrl, scope);
                using (Process.Start(redirectUrl))
                {
                    for (var times = 0; times < 2; times++)
                    {
                        var task = redirectListener.GetContextAsync();
                        var timeoutTask = cancelToken != null ? Task.Delay(timeout, cancelToken.Value) : Task.Delay(timeout);
                        var anytask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);
                        if (anytask == task)
                        {
                            var context = await task.ConfigureAwait(false);
                            if (times == 0)
                            {
                                var loginResponse = Encoding.UTF8.GetBytes(string.Format(DefaultOpenAuthResponse, loginurl));
                                await SendResponse(context.Response, loginResponse).ConfigureAwait(false);
                            }
                            else
                            {
                                await ProcessRedirect(context, redirectUrl).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            if (timeoutTask.IsCanceled)
                            {
                                return false;
                            }

                            throw new TimeoutException("No redirection detected");
                        }
                    }
                }
            }

            return authTokens != null;
        }

        private static async Task SendResponse(HttpListenerResponse response, byte[] body)
        {
            response.StatusCode = 200;
            response.ContentLength64 = body.Length;
            using (var output = response.OutputStream)
            {
                await output.WriteAsync(body, 0, body.Length);
            }
        }

        private static string ScopeToString(CloudDriveScopes scope) => string.Join(" ", Enum.GetValues(typeof(CloudDriveScopes)).Cast<CloudDriveScopes>().Where(v => scope.HasFlag(v)).Select(v => ScopeToStringMap[v]));

        private void CallOnTokenUpdate(string accessToken, string refreshToken, DateTime expiresIn)
        {
            ITokenUpdateListener action;
            if ((weakOnTokenUpdate != null) && weakOnTokenUpdate.TryGetTarget(out action))
            {
                action.OnTokenUpdated(accessToken, refreshToken, expiresIn);
            }
        }

        private HttpListener CreateListener(string redirectUrl, out string realRedirectUrl, Func<int, int, int> portSelector = null)
        {
            var listener = new HttpListener();
            var port = 0;
            var time = 0;
            while (true)
            {
                try
                {
                    port = (portSelector ?? DefaultPortSelector).Invoke(port, time++);
                    realRedirectUrl = string.Format(CultureInfo.InvariantCulture, redirectUrl, port);
                    listener.Prefixes.Add(realRedirectUrl);
                    return listener;
                }
                catch (HttpListenerException)
                {
                    // Skip, try another port
                }
                catch (Exception)
                {
                    listener.Close();
                    throw;
                }
            }
        }

        private int DefaultPortSelector(int lastPort, int time)
        {
            if (time == 0)
            {
                return ListenerPortStart;
            }

            if (time > 2)
            {
                throw new InvalidOperationException("Cannot select port for redirect url");
            }

            return lastPort + 1;
        }

        private async Task<string> GetContentUrl() => (await Account.GetEndpoint()).contentUrl;

        private async Task<string> GetMetadataUrl() => (await Account.GetEndpoint()).metadataUrl;

        private async Task<string> GetToken()
        {
            if (authTokens == null)
            {
                throw new InvalidOperationException("Not authenticated");
            }

            if (authTokens.IsExpired)
            {
                await UpdateToken();
            }

            return authTokens?.access_token;
        }

        private async Task ProcessRedirect(HttpListenerContext context, string redirectUrl)
        {
            var error = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("error_description");

            if (error != null)
            {
                throw new InvalidOperationException(error);
            }

            var code = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("code");

            await SendResponse(context.Response, CloseTabResponse);

            await AuthenticationByCode(code, redirectUrl);
        }

        private async Task SettingsSetter(HttpWebRequest client)
        {
            if (authTokens != null && client.RequestUri.AbsoluteUri != TokenUrl)
            {
                client.Headers.Add("Authorization", "Bearer " + await GetToken());
            }

            client.CachePolicy = StandartCache;
            client.UserAgent = "AZIACDDokanNet/" + GetType().Assembly.ImageRuntimeVersion;

            client.Timeout = 15000;

            client.AllowReadStreamBuffering = false;
            client.AllowWriteStreamBuffering = true;
            client.AutomaticDecompression = DecompressionMethods.GZip;
            client.PreAuthenticate = true;
            client.UseDefaultCredentials = true;

            if (Proxy != null)
            {
                client.Proxy = Proxy;
            }
        }

        private async Task UpdateToken()
        {
            var free = await tokenUpdateSem.WaitAsync(0);
            if (!free)
            {
                await tokenUpdateSem.WaitAsync();
                tokenUpdateSem.Release();
                return;
            }

            try
            {
                var form = new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", authTokens.refresh_token },
                        { "client_id", clientId },
                        { "client_secret", clientSecret }
                    };
                var newtoken = await http.PostForm<AuthToken>(TokenUrl, form);
                if (newtoken != null)
                {
                    authTokens = newtoken;
                    CallOnTokenUpdate(authTokens.access_token, authTokens.refresh_token, DateTime.UtcNow.AddSeconds(authTokens.expires_in));
                }
            }
            finally
            {
                tokenUpdateSem.Release();
            }
        }
    }
}
