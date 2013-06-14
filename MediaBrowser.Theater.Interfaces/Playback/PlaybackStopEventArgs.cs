using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Class PlaybackStopEventArgs
    /// </summary>
    public class PlaybackStopEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        /// <value>The player.</value>
        public IMediaPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        /// <value>The playlist.</value>
        public List<BaseItemDto> Playlist { get; set; }

        /// <summary>
        /// Gets or sets the index of the ending playlist.
        /// </summary>
        /// <value>The index of the ending playlist.</value>
        public int EndingPlaylistIndex { get; set; }

        /// <summary>
        /// Gets or sets the ending media.
        /// </summary>
        /// <value>The ending media.</value>
        public BaseItemDto EndingMedia { get; set; }

        /// <summary>
        /// Gets or sets the ending position ticks.
        /// </summary>
        /// <value>The ending position ticks.</value>
        public long? EndingPositionTicks { get; set; }
    }
}
