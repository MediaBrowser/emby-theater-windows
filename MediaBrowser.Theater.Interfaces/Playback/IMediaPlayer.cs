using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IMediaPlayer
    /// </summary>
    public interface IMediaPlayer
    {
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
        /// Gets a value indicating whether this instance can set which audio stream to play
        /// </summary>
        /// <value><c>true</c> if this instance can set the audio stream index; otherwise, <c>false</c>.</value>
        bool CanSetAudioStreamIndex { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can set which subtitle stream to play
        /// </summary>
        /// <value><c>true</c> if this instance can set the subtitle stream index; otherwise, <c>false</c>.</value>
        bool CanSetSubtitleStreamIndex { get; }

        /// <summary>
        /// Gets a value indicating whether this instance accepts navigation commands 
        /// Up, down, left, right, pageup, pagedown, goHome, goSettings
        /// </summary>
        /// <value><c>true</c> if this instance can set the subtitle stream index; otherwise, <c>false</c>.</value>
        bool CanAcceptNavigationCommands { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can queue.
        /// </summary>
        /// <value><c>true</c> if this instance can queue; otherwise, <c>false</c>.</value>
        bool CanQueue { get; }

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
        /// Gets the current duration ticks.
        /// </summary>
        /// <value>The current duration ticks.</value>
        long? CurrentDurationTicks { get; }

        /// <summary>
        /// Get the current subtitle index.
        /// </summary>
        /// <value>The current subtitle index.</value>
        int? CurrentSubtitleStreamIndex { get; }

        /// <summary>
        /// Get the current audio index.
        /// </summary>
        /// <value>The current audio index.</value>
        int? CurrentAudioStreamIndex { get; }
      
        /// <summary>
        /// Determines whether this instance can play the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance can play the specified item; otherwise, <c>false</c>.</returns>
        bool CanPlayByDefault(BaseItemDto item);

        /// <summary>
        /// Determines whether this instance [can play media type] the specified media type.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <returns><c>true</c> if this instance [can play media type] the specified media type; otherwise, <c>false</c>.</returns>
        bool CanPlayMediaType(string mediaType);

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        Task Play(PlayOptions options);

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        void Pause();

        /// <summary>
        /// Uns the pause.
        /// </summary>
        void UnPause();

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        void Seek(long positionTicks);

        /// <summary>
        /// Changes the track.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        void ChangeTrack(int newIndex);

        // <summary>
        // Changes to the next track.
        // <summary>
        void NextTrack();

        // <summary>
        // Changes to the previous track.
        // </summary>
        void PreviousTrack();


        /// <summary>
        /// Set the play rate - FF or Rewindw
        /// </summary>
        /// <param name="rate">The speed to play the media.</param>
        void SetRate(Double rate);

        /// <summary>
        /// Set subtitle by subtitleStreamIndex
        /// </summary>
        /// <param name="subtitleStreamIndex">Index of desired subtitle.</param>
        void SetSubtitleStreamIndex(int subtitleStreamIndex);
        
        /// <summary>
        /// Advances to teh next subtitle stream, Wraps at the end
        /// </summary>
        void NextSubtitleStream();

        /// <summary>
        /// Set subtitle by subtitleStreamIndex
        /// </summary>
        /// <param name="audioStreamIndex">Index of desired audio stream.</param>
        void SetAudioStreamIndex(int audioStreamIndex);

        /// <summary>
        /// Advances to the next Audio Stream, Wraps at the end
        /// </summary>
        void NextAudioStream();
    }
}
