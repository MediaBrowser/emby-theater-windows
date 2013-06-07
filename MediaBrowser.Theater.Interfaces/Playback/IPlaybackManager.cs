using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IPlaybackManager
    /// </summary>
    public interface IPlaybackManager
    {
        /// <summary>
        /// Occurs when [playback started].
        /// </summary>
        event EventHandler<PlaybackStartEventArgs> PlaybackStarted;
        /// <summary>
        /// Occurs when [playback completed].
        /// </summary>
        event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="mediaPlayers">The media players.</param>
        void AddParts(IEnumerable<IMediaPlayer> mediaPlayers);

        /// <summary>
        /// Gets the media players.
        /// </summary>
        /// <value>The media players.</value>
        IEnumerable<IMediaPlayer> MediaPlayers { get; }

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        Task Play(PlayOptions options);

        /// <summary>
        /// Stops all playback.
        /// </summary>
        /// <returns>Task.</returns>
        Task StopAllPlayback();
    }
}
