using System;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Theater.Api.Configuration
{
    /// <summary>
    ///     This is the UI's device configuration that applies regardless of which user is logged in.
    /// </summary>
    public class ApplicationConfiguration : BaseApplicationConfiguration
    {
        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the auto-login user details.
        /// </summary>
        /// <value>The auto-login details.</value>
        public AutoLoginConfiguration AutoLoginConfiguration { get; set; }

        /// <summary>
        ///     Gets or sets the WOL (Wake On LAN) configuration.
        /// </summary>
        /// <value>The WOL configuration.</value>
        public WolConfiguration WakeOnLanConfiguration { get; set; }

        /// <summary>
        ///     Gets or sets the internal media player configuration.
        /// </summary>
        public InternalPlayerConfiguration InternalPlayerConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the player configurations.
        /// </summary>
        /// <value>The player configurations.</value>
        public PlayerConfiguration[] MediaPlayers { get; set; }

        public bool DownloadCompressedImages { get; set; }

        public Guid ActiveThemeGuid { get; set; }

        public bool EnableHighQualityImageScaling { get; set; }

        public int MaxStreamingBitrate { get; set; }

        public ApplicationConfiguration()
        {
            // default sever and theme settings
            ServerAddress = "http://localhost:8096";
            ActiveThemeGuid = new Guid("C501C937-3BC9-471A-A538-20FAA9B7CE51");
            EnableHighQualityImageScaling = true;
            MaxStreamingBitrate = 3000000;

            AutoLoginConfiguration = new AutoLoginConfiguration();
            WakeOnLanConfiguration = new WolConfiguration();
            InternalPlayerConfiguration = new InternalPlayerConfiguration();
            MediaPlayers = new PlayerConfiguration[0];
        }
    }
}