
namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Interface IExternalMediaPlayer
    /// </summary>
    public interface IExternalMediaPlayer : IMediaPlayer
    {
        bool RequiresConfiguredPath { get; }
        bool RequiresConfiguredArguments { get; }
        bool CanCloseAutomaticallyOnStopButton { get; }
    }
}
