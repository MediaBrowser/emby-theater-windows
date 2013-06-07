using System;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Class PlaybackStartEventArgs
    /// </summary>
    public class PlaybackStartEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        /// <value>The player.</value>
        public IMediaPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        public PlayOptions Options { get; set; }
    }
}
