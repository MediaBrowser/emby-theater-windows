using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaybackManager
    {
        IPlayQueue Queue { get; }
        Task<IPlaybackSessionAccessor> GetSessionLock();
        IObservable<PlaybackStatus> Events { get; }
    }
}
