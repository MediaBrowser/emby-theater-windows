
namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IExternalMediaPlayer
    /// </summary>
    public interface IExternalMediaPlayer : IMediaPlayer
    {
        /// <summary>
        /// Gets a value indicating whether this instance can close automatically on stop button.
        /// </summary>
        /// <value><c>true</c> if this instance can close automatically on stop button; otherwise, <c>false</c>.</value>
        bool CanCloseAutomaticallyOnStopButton { get; }
    }
}
