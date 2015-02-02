using System;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaybackSessionAccessor : IDisposable
    {
        IPlaybackSession Session { get; }
    }
}