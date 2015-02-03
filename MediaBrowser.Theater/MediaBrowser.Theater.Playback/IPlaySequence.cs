using System;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaySequence : IDisposable
    {
        Media Current { get; }
        bool Next();
        bool Previous();
        bool SkipTo(int index);
    }
}