
namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class PlayerConfiguration
    /// </summary>
    public class PlayerConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        /// <value>The name of the player.</value>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets the type of the media.
        /// </summary>
        /// <value>The type of the media.</value>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the game system.
        /// </summary>
        /// <value>The game system.</value>
        public string GameSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the file extensions.
        /// </summary>
        /// <value>The file extensions.</value>
        public string[] FileExtensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [play bluray].
        /// </summary>
        /// <value><c>true</c> if [play bluray]; otherwise, <c>false</c>.</value>
        public bool PlayBluray { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [play DVD].
        /// </summary>
        /// <value><c>true</c> if [play DVD]; otherwise, <c>false</c>.</value>
        public bool PlayDvd { get; set; }

        /// <summary>
        /// Gets or sets the video formats.
        /// </summary>
        /// <value>The video formats.</value>
        public bool Play3DVideo { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the args.
        /// </summary>
        /// <value>The args.</value>
        public string Args { get; set; }

        public IsoConfiguration IsoMethod { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether [close on stop button].
        /// </summary>
        /// <value><c>true</c> if [close on stop button]; otherwise, <c>false</c>.</value>
        public bool CloseOnStopButton { get; set; }

        public PlayerConfiguration()
        {
            FileExtensions = new string[] { };
            Play3DVideo = true;
            PlayBluray = true;
            PlayDvd = true;
        }
    }

    public enum IsoConfiguration
    {
        None,
        Mount,
        PassThrough
    }
}
