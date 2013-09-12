
namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IConfigurableMediaPlayer
    /// </summary>
    public interface IConfigurableMediaPlayer : IMediaPlayer
    {
        /// <summary>
        /// Gets a value indicating whether [requires configured path].
        /// </summary>
        /// <value><c>true</c> if [requires configured path]; otherwise, <c>false</c>.</value>
        bool RequiresConfiguredPath { get; }
        /// <summary>
        /// Gets a value indicating whether [requires configured arguments].
        /// </summary>
        /// <value><c>true</c> if [requires configured arguments]; otherwise, <c>false</c>.</value>
        bool RequiresConfiguredArguments { get; }
        /// <summary>
        /// Gets a value indicating whether this instance can close automatically on stop button.
        /// </summary>
        /// <value><c>true</c> if this instance can close automatically on stop button; otherwise, <c>false</c>.</value>
        bool CanCloseAutomaticallyOnStopButton { get; }
    }
}
