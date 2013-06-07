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
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public List<BaseItemDto> Items { get; set; }
    }
}
