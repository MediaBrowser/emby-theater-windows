using System;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public enum SessionCompletion
    {
        Complete,
        Stopped
    }

    public interface IPreparedSessions
    {
        Task<SessionCompletion> Start();
        IObservable<IPlaybackSession> Sessions { get; }
        IObservable<PlaybackStatus> Status { get; }
    }
}