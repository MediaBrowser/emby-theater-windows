using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Class MediaChangeEventArgs
    /// </summary>
    public class MediaChangeEventArgs
    {
        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        /// <value>The player.</value>
        public IMediaPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the index of the previous playlist.
        /// </summary>
        /// <value>The index of the previous playlist.</value>
        public int PreviousPlaylistIndex { get; set; }

        /// <summary>
        /// Gets or sets the new index of the playlist.
        /// </summary>
        /// <value>The new index of the playlist.</value>
        public int NewPlaylistIndex { get; set; }

        /// <summary>
        /// Gets or sets the previous media.
        /// </summary>
        /// <value>The previous media.</value>
        public BaseItemDto PreviousMedia { get; set; }

        /// <summary>
        /// Gets or sets the previous stream information.
        /// </summary>
        /// <value>The previous stream information.</value>
        public StreamInfo PreviousStreamInfo { get; set; }

        /// <summary>
        /// Gets or sets the new media.
        /// </summary>
        /// <value>The new media.</value>
        public BaseItemDto NewMedia { get; set; }
        
        /// <summary>
        /// Gets or sets the ending position ticks.
        /// </summary>
        /// <value>The ending position ticks.</value>
        public long? EndingPositionTicks { get; set; }
    }
}
