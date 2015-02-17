using System.Threading.Tasks;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public interface ITransportControls
    {
        void Play();
        void Pause();
        //Task Stop();
        void Seek(long ticks);
        void SkipNext();
        void SkipPrevious();
        void SkipTo(int itemIndex);
        void SelectStream(MediaStreamType channel, int index);
        void SetPlaybackSpeed(double speedMultiplier);
    }
}