using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Root class for API. Provides authentication methods.
    /// </summary>
    public class AmazonDrive
    {
        internal static readonly TimeSpan generalExpiration = TimeSpan.FromMinutes(5);
        const string loginUrlBase = "https://www.amazon.com/ap/oa";

        private string clientId;
        private string clientSecret;

        private CloudDriveScope scope;
        private AuthToken token;

        internal readonly Tools.HttpClient http;

        /// <summary>
        /// Account related part of API
        /// </summary>
        public readonly AmazonAccount Account;

        /// <summary>
        /// Nodes management part of API
        /// </summary>
        public readonly AmazonNodes Nodes;

        /// <summary>
        /// File upload and download part of API
        /// </summary>
        public readonly AmazonFiles Files;

        /// <summary>
        /// Start of 3 ports range used in localhost redirect listener for authentication with default port selector
        /// </summary>
        public int ListenerPortStart { get; set; } = 45674;

        /// <summary>
        /// Authenticate using know auth token, renew token and expiration time
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="authRenewToken"></param>
        /// <param name="authTokenExpiration"></param>
        /// <returns>True if authenticated</returns>
        public async Task<bool> Authentication(string authToken, string authRenewToken, DateTime authTokenExpiration)
        {
            token = new AuthToken
            {
                expires_in = 0,
                createdTime = authTokenExpiration,
                access_token = authToken,
                refresh_token = authRenewToken,
                token_type = "bearer"
            };
            await UpdateToken().ConfigureAwait(false);
            return token != null;
        }

        /// <summary>
        /// Creates instance of API
        /// </summary>
        /// <param name="clientId">Your App ClientID. From Amazon Developers Console.</param>
        /// <param name="clientSecret">Your App Secret. From Amazon Developers Console.</param>
        public AmazonDrive(string clientId, string clientSecret)
        {
            this.clientSecret = clientSecret;
            this.clientId = clientId;
            http = new Tools.HttpClient(SettingsSetter);
            Account = new AmazonAccount(this);
            Nodes = new AmazonNodes(this);
            Files = new AmazonFiles(this);
        }


        private static string BuildLoginUrl(string clientId, string redirectUrl, CloudDriveScope scope)
        {
            Contract.Assert(redirectUrl != null);

            return $"{loginUrlBase}?client_id={clientId}&scope={ScopeToString(scope)}&response_type=code&redirect_uri={redirectUrl}";

        }

        internal async Task<string> GetContentUrl() => (await Account.GetEndpoint().ConfigureAwait(false)).contentUrl;
        internal async Task<string> GetMetadataUrl() => (await Account.GetEndpoint().ConfigureAwait(false)).metadataUrl;

        readonly RequestCachePolicy standartCache = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

        private async Task SettingsSetter(HttpWebRequest client)
        {
            if (token != null && !updatingToken)
                client.Headers.Add("Authorization", "Bearer " + await GetToken().ConfigureAwait(false));
            client.CachePolicy = standartCache;
            client.UserAgent = "AZIACDDokanNet/" + this.GetType().Assembly.ImageRuntimeVersion;

            client.Timeout = 15000;

            client.AllowReadStreamBuffering = false;
            client.AllowWriteStreamBuffering = true;
            client.AutomaticDecompression = DecompressionMethods.GZip;
            client.PreAuthenticate = true;
            client.UseDefaultCredentials = true;
        }

        private async Task<string> GetToken()
        {
            if (token == null) throw new InvalidOperationException("Not authenticated");
            if (token.IsExpired) await UpdateToken().ConfigureAwait(false);
            return token?.access_token;
        }

        bool updatingToken = false;
        private async Task UpdateToken()
        {
            updatingToken = true;
            var form = new Dictionary<string, string>
                    {
                        {"grant_type","refresh_token" },
                        {"refresh_token",token.refresh_token},
                        {"client_id",clientId},
                        {"client_secret",clientSecret}
                    };
            token = await http.PostForm<AuthToken>("https://api.amazon.com/auth/o2/token", form).ConfigureAwait(false);
            if (token != null)
                OnTokenUpdate?.Invoke(token.access_token, token.refresh_token, DateTime.UtcNow.AddSeconds(token.expires_in));
            updatingToken = false;
        }

        static readonly Regex browserPathPattern = new Regex("^(?<path>[^\" ]+)|\"(?<path>[^\"]+)\" (?<args>.*)$");

        private Process OpenUrlInDefaultBrowser(string url)
        {
            using (var nameKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.html\UserChoice", false))
            {
                var appName = nameKey.GetValue("Progid") as string;
                using (var commandKey = Registry.ClassesRoot.OpenSubKey($@"{appName}\shell\open\command", false))
                {
                    var str = commandKey.GetValue(null) as string;
                    var m = browserPathPattern.Match(str);
                    if (!m.Success || !m.Groups["path"].Success) throw new InvalidOperationException("Can not find default browser path");
                    var path = m.Groups["path"].Value;
                    var args = m.Groups["args"].Value.Replace("%1", url);
                    return Process.Start(path, args);
                }
            }
        }

        private int DefaultPortSelector(int lastPort, int time)
        {
            if (time == 0) return ListenerPortStart;
            if (time > 2) throw new InvalidOperationException("Cannot select port for redirect url");
            return lastPort + 1;

        }

        private HttpListener redirectListener;
        private string redirectUrl;

        private void CreateListener(Func<int, int, int> portSelector = null)
        {
            if (redirectListener != null)
            {
                redirectListener.Close();
            }

            var listener = new HttpListener();
            int port = 0;
            int time = 0;
            while (true)
            {
                try
                {
                    port = (portSelector ?? DefaultPortSelector).Invoke(port, time++);
                    redirectUrl = $"http://localhost:{port}/signin/";
                    listener.Prefixes.Add(redirectUrl);
                    redirectListener = listener;
                    return;
                }
                catch (HttpListenerException)
                {
                    //Skip, try another port
                }
                catch (InvalidOperationException)
                {
                    listener.Close();
                    throw;
                }
            }
        }

        /// <summary>
        /// Opens Amazon Cloud Drive authentication in default browser. Then it starts listener for port 45674 if portSelector is null
        /// </summary>
        /// <param name="scope">Your App scope to access cloud</param>
        /// <param name="timeout">How long lister will wait for redirect before throw TimeoutException</param>
        /// <param name="portSelector">Func to select port for redirect listener. 
        /// portSelector(int lastPort, int time) where lastPost is port used last time and 
        /// time is number of times selector was called before. To abort port selection throw exception other than HttpListenerException</param>
        /// <param name="cancelToken">Cancellation for auth. Can be null.</param>
        /// <returns>True if authenticated</returns>
        /// <exception cref="InvalidOperationException">if any selected port could not be opened by default selector</exception>
        public async Task<bool> SafeAuthenticationAsync(CloudDriveScope scope, TimeSpan timeout, CancellationToken? cancelToken = null, Func<int, int, int> portSelector = null)
        {
            CreateListener(portSelector);

            redirectListener.Start();
            using (var tabProcess = Process.Start(BuildLoginUrl(clientId, redirectUrl, scope)))
            {
                try
                {
                    var task = redirectListener.GetContextAsync();
                    var timeoutTask = (cancelToken != null) ? Task.Delay(timeout, cancelToken.Value) : Task.Delay(timeout);
                    var anytask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);
                    if (anytask == task)
                    {
                        await ProcessRedirect(await task, clientId, clientSecret, redirectUrl).ConfigureAwait(false);

                        this.scope = scope;
                    }
                    else
                    {
                        if (timeoutTask.IsCanceled) return false;
                        throw new TimeoutException("No redirection detected");
                    }
                }
                finally
                {
                    redirectListener.Close();
                    redirectListener = null;
                    //tabProcess.Kill();
                }
            }

            return token != null;
        }

        private async Task ProcessRedirect(HttpListenerContext context, string clientId, string secret, string redirectUrl)
        {
            var error = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("error_description");

            if (error != null)
            {
                throw new InvalidOperationException(error);
            }

            var code = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("code");

            await SendRedirectResponse(context.Response).ConfigureAwait(false);

            var form = new Dictionary<string, string>
                                {
                                    { "grant_type","authorization_code" },
                                    {"code",code},
                                    {"client_id",clientId},
                                    {"client_secret",secret},
                                    {"redirect_uri",redirectUrl}
                                };
            token = await http.PostForm<AuthToken>("https://api.amazon.com/auth/o2/token", form).ConfigureAwait(false);
            if (token != null)
                OnTokenUpdate?.Invoke(token.access_token, token.refresh_token, DateTime.UtcNow.AddSeconds(token.expires_in));

            await Account.GetEndpoint().ConfigureAwait(false);
        }

        readonly byte[] closeTabResponse = Encoding.UTF8.GetBytes("<SCRIPT>window.open('', '_parent','');window.close();</SCRIPT>You can close this tab");

        private async Task SendRedirectResponse(HttpListenerResponse response)
        {
            response.StatusCode = 200;
            response.ContentLength64 = closeTabResponse.Length;
            await response.OutputStream.WriteAsync(closeTabResponse, 0, closeTabResponse.Length).ConfigureAwait(false);
            response.OutputStream.Close();
        }

        static readonly Dictionary<CloudDriveScope, string> scopeToStringMap = new Dictionary<CloudDriveScope, string>
        {
            {CloudDriveScope.ReadImage,"clouddrive:read_image" },
            {CloudDriveScope.ReadVideo,"clouddrive:read_video" },
            {CloudDriveScope.ReadDocument,"clouddrive:read_document" },
            {CloudDriveScope.ReadOther,"clouddrive:read_other" },
            {CloudDriveScope.ReadAll,"clouddrive:read_all" },
            {CloudDriveScope.Write,"clouddrive:write" }
        };

        /// <summary>
        /// Callback called when auth token get updated on authentication or renewal.
        /// </summary>
        public Action<string, string, DateTime> OnTokenUpdate { get; set; }

        private static string ScopeToString(CloudDriveScope scope)
        {
            var result = new List<string>();
            var values = Enum.GetValues(typeof(CloudDriveScope));
            foreach (CloudDriveScope value in values)
                if (scope.HasFlag(value))
                    result.Add(scopeToStringMap[value]);
            return string.Join(" ", result);
        }

    }
}
