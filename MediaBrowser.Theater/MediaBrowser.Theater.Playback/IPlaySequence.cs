using System;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlaySequence<out T> : IDisposable
    {
        T Current { get; }
        int CurrentIndex { get; }
        Task<bool> Next();
        Task<bool> Previous();
        Task<bool> SkipTo(int index);
    }
}