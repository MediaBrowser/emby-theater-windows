using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Theater.Api.Configuration
{
    /// <summary>
    ///     This is the UI's device configuration that applies regardless of which user is logged in.
    /// </summary>
    public class ApplicationConfiguration : BaseApplicationConfiguration
    {
        /// <summary>
        ///     Gets or sets the server host name (myserver or 192.168.x.x)
        /// </summary>
        /// <value>The name of the server host.</value>
        public string ServerHostName { get; set; }

        /// <summary>
        ///     Gets or sets the port number used by the API
        /// </summary>
        /// <value>The server API port.</value>
        public int ServerApiPort { get; set; }

        /// <summary>
        ///     Gets or sets the auto-login user details.
        /// </summary>
        /// <value>The auto-login details.</value>
        public AutoLoginConfiguration AutoLoginConfiguration { get; set; }

        public bool DownloadCompressedImages { get; set; }
    }
}