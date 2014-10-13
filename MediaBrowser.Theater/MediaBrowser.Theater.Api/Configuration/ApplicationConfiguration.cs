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
        ///     Gets or sets the internal media player configuration.
        /// </summary>
        public InternalPlayerConfiguration InternalPlayerConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the player configurations.
        /// </summary>
        /// <value>The player configurations.</value>
        public PlayerConfiguration[] MediaPlayers { get; set; }

        public Guid ActiveThemeGuid { get; set; }

        public bool EnableHighQualityImageScaling { get; set; }

        public int MaxStreamingBitrate { get; set; }

        public bool RememberLogin { get; set; }

        public ApplicationConfiguration()
        {
            // default sever and theme settings
            ActiveThemeGuid = new Guid("C501C937-3BC9-471A-A538-20FAA9B7CE51");
            EnableHighQualityImageScaling = true;
            MaxStreamingBitrate = 3000000;

            InternalPlayerConfiguration = new InternalPlayerConfiguration();
            MediaPlayers = new PlayerConfiguration[0];
        }
    }
}