using System;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaybackSession : ITransportControls
    {
        IObservable<PlaybackStatus> Events { get; }
        PlaybackCapabilities Capabilities { get; }
        PlaybackStatus Status { get; }
    }
}