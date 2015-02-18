using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public interface IMediaPlayer
    {
        int Priority { get; }
        string Name { get; }
        bool CanPlay(Media media);
        bool PrefersBackgroundPlayback { get; }
        Task<IPreparedSessions> Prepare(IPlaySequence sequence, CancellationToken cancellationToken);
    }
}