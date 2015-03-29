using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public interface IMediaPlayer
    {
        int Priority { get; }
        string Name { get; }
        Task<PlayableMedia> GetPlayable(Media media);
        bool PrefersBackgroundPlayback { get; }
        Task<IPreparedSessions> Prepare(IPlaySequence<PlayableMedia> sequence, CancellationToken cancellationToken);
        Task Startup();
        Task Shutdown();
    }
}