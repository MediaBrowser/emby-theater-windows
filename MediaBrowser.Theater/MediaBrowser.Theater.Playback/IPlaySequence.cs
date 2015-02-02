using System;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaySequence : IDisposable
    {
        BaseItemDto Current { get; }
        bool Next();
        bool Previous();
    }
}