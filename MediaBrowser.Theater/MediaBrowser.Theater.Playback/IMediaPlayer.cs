using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public interface IMediaPlayer
    {
        int Priority { get; }
        string Name { get; }
        bool CanPlay(BaseItemDto media);
        Task<IPreparedSessions> Prepare(IPlaySequence sequence);
    }

    public interface IPreparedSessions
    {
        Task Start();
        IObservable<IPlaybackSession> Sessions { get; }
        IObservable<PlaybackStatus> Status { get; }
    }
}