using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaybackManager
    {
        List<IMediaPlayer> Players { get; }
        IPlayQueue Queue { get; }
        GlobalPlaybackSettings GlobalSettings { get; }
        IObservable<PlaybackStatus> Events { get; }
        IObservable<IPlaybackSession> Sessions { get; }
        Task Initialize();
        Task Shutdown();
        Task<bool> AccessSession(Func<IPlaybackSession, Task> action);
        Task BeginPlayback(int startIndex = 0);
        Task StopPlayback();
        Task<IEnumerable<BaseItemDto>> GetIntros(BaseItemDto item);

        event Action PlaybackStarting;
        event Action PlaybackFinished;
    }
}