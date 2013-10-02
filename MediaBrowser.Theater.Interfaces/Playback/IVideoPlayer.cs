using System.Collections.Generic;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IVideoPlayer
    /// </summary>
    public interface IVideoPlayer : IMediaPlayer
    {
        /// <summary>
        /// Gets a value indicating whether this instance can select audio track.
        /// </summary>
        /// <value><c>true</c> if this instance can select audio track; otherwise, <c>false</c>.</value>
        bool CanSelectAudioTrack { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can select subtitle track.
        /// </summary>
        /// <value><c>true</c> if this instance can select subtitle track; otherwise, <c>false</c>.</value>
        bool CanSelectSubtitleTrack { get; }

        /// <summary>
        /// Gets the subtitle tracks.
        /// </summary>
        /// <value>The subtitle tracks.</value>
        IReadOnlyList<SelectableMediaStream> SelectableStreams { get; }

        /// <summary>
        /// Changes the audio track.
        /// </summary>
        /// <param name="track">The track.</param>
        void ChangeAudioStream(SelectableMediaStream track);

        /// <summary>
        /// Changes the subtitle track.
        /// </summary>
        /// <param name="track">The track.</param>
        void ChangeSubtitleStream(SelectableMediaStream track);

        /// <summary>
        /// Removes the subtitles.
        /// </summary>
        void RemoveSubtitles();
    }
}
