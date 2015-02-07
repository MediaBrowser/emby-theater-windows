using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaybackManager
    {
        IList<IMediaPlayer> Players { get; }
        IPlayQueue Queue { get; }
        GlobalPlaybackSettings GlobalSettings { get; }
        IObservable<PlaybackStatus> Events { get; }
        Task<IPlaybackSessionAccessor> GetSessionLock();
    }
}