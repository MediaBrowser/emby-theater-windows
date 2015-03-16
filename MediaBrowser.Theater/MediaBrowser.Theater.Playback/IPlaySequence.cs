using System;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaySequence : IDisposable
    {
        Media Current { get; }
        int CurrentIndex { get; }
        bool Next();
        bool Previous();
        bool SkipTo(int index);
    }
}