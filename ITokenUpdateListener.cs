using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Listener for Authentication Token updates
    /// </summary>
    public interface ITokenUpdateListener
    {
        /// <summary>
        /// Called when Authentication Token updated
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="refresh_token"></param>
        /// <param name="expires_in"></param>
        void OnTokenUpdated(string access_token, string refresh_token, DateTime expires_in);
    }
}
