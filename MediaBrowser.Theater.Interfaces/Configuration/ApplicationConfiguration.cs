using MediaBrowser.Model.Configuration;
using System.Windows;

namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// This is the UI's device configuration that applies regardless of which user is logged in.
    /// </summary>
    public class ApplicationConfiguration : BaseApplicationConfiguration
    {
        /// <summary>
        /// Gets or sets the player configurations.
        /// </summary>
        /// <value>The player configurations.</value>
        public PlayerConfiguration[] MediaPlayers { get; set; }

        /// <summary>
        /// Gets or sets the state of the window.
        /// </summary>
        /// <value>The state of the window.</value>
        public WindowState? WindowState { get; set; }

        /// <summary>
        /// Gets or sets the window top.
        /// </summary>
        /// <value>The window top.</value>
        public double? WindowTop { get; set; }

        /// <summary>
        /// Gets or sets the window left.
        /// </summary>
        /// <value>The window left.</value>
        public double? WindowLeft { get; set; }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        /// <value>The width of the window.</value>
        public double? WindowWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the window.
        /// </summary>
        /// <value>The height of the window.</value>
        public double? WindowHeight { get; set; }

        public bool EnableHighQualityImageScaling { get; set; }

        public bool EnableBackdrops { get; set; }

        public InternalPlayerConfiguration InternalPlayerConfiguration { get; set; }

        public int MaxStreamingBitrate { get; set; }

        public VlcConfiguration VlcConfiguration { get; set; }

        public bool RememberLogin { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationConfiguration" /> class.
        /// </summary>
        public ApplicationConfiguration()
            : base()
        {
            EnableBackdrops = true;
            MediaPlayers = new PlayerConfiguration[] { };

            InternalPlayerConfiguration = new InternalPlayerConfiguration();

            MaxStreamingBitrate = 3000000;

            VlcConfiguration = new VlcConfiguration();
        }
    }
}
