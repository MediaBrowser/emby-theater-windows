using System;

namespace MediaBrowser.Theater.Playback
{
    public interface IPreparedSessions
    {
        void Start();
        IObservable<IPlaybackSession> Sessions { get; }
        IObservable<PlaybackStatus> Status { get; }
    }
}