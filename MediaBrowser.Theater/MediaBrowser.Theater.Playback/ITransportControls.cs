using System.Threading.Tasks;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public interface ITransportControls
    {
        void Play();
        void Pause();
        Task Stop();
        void Seek(long ticks);
        void SkipNext();
        void SkipPrevious();
        void SelectStream(MediaStreamType channel, int index);
        void SetVolume(decimal volume);
        void SetMuted(bool muted);
    }
}