using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Theater.Interfaces.Configuration;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IMediaPlayer
    /// </summary>
    public interface IMediaPlayer
    {
        /// <summary>
        /// Occurs when [volume changed].
        /// </summary>
        event EventHandler VolumeChanged;

        /// <summary>
        /// Occurs when [play state changed].
        /// </summary>
        event EventHandler PlayStateChanged;

        /// <summary>
        /// Occurs when [media changed].
        /// </summary>
        event EventHandler<MediaChangeEventArgs> MediaChanged;

        /// <summary>
        /// Occurs when [playback completed].
        /// </summary>
        event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;
        
        /// <summary>
        /// Gets the playlist.
        /// </summary>
        /// <value>The playlist.</value>
        IReadOnlyList<BaseItemDto> Playlist { get; }

        /// <summary>
        /// Gets the current play options.
        /// </summary>
        /// <value>The current play options.</value>
        PlayOptions CurrentPlayOptions { get; }

        /// <summary>
        /// Gets the index of the current playlist.
        /// </summary>
        /// <value>The index of the current playlist.</value>
        int CurrentPlaylistIndex { get; }

        /// <summary>
        /// Gets the current media.
        /// </summary>
        /// <value>The current media.</value>
        BaseItemDto CurrentMedia { get; }
        
        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        bool CanSeek { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can pause.
        /// </summary>
        /// <value><c>true</c> if this instance can pause; otherwise, <c>false</c>.</value>
        bool CanPause { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can queue.
        /// </summary>
        /// <value><c>true</c> if this instance can queue; otherwise, <c>false</c>.</value>
        bool CanQueue { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can control volume.
        /// </summary>
        /// <value><c>true</c> if this instance can control volume; otherwise, <c>false</c>.</value>
        bool CanControlVolume { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can track progress.
        /// </summary>
        /// <value><c>true</c> if this instance can track progress; otherwise, <c>false</c>.</value>
        bool CanTrackProgress { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether [supports multi file playback].
        /// </summary>
        /// <value><c>true</c> if [supports multi file playback]; otherwise, <c>false</c>.</value>
        bool SupportsMultiFilePlayback { get; }

        /// <summary>
        /// Gets the state of the play.
        /// </summary>
        /// <value>The state of the play.</value>
        PlayState PlayState { get; }

        /// <summary>
        /// Gets the current position ticks.
        /// </summary>
        /// <value>The current position ticks.</value>
        long? CurrentPositionTicks { get; }

        /// <summary>
        /// Determines whether this instance can play the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance can play the specified item; otherwise, <c>false</c>.</returns>
        bool CanPlay(BaseItemDto item);

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns>Task.</returns>
        Task Stop();

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        Task Play(PlayOptions options);

        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value><c>true</c> if this instance is muted; otherwise, <c>false</c>.</value>
        bool IsMuted { get; }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>The volume.</value>
        int Volume { get; }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>Task.</returns>
        Task SetVolume(int volume);

        /// <summary>
        /// Mutes this instance.
        /// </summary>
        /// <returns>Task.</returns>
        Task Mute();

        /// <summary>
        /// Uns the mute.
        /// </summary>
        /// <returns>Task.</returns>
        Task UnMute();

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <returns>Task.</returns>
        Task Pause();

        /// <summary>
        /// Uns the pause.
        /// </summary>
        /// <returns>Task.</returns>
        Task UnPause();

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        Task Seek(long positionTicks);
    }
}
