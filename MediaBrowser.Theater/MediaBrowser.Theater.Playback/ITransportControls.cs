using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public interface ITransportControls
    {
        void Play();
        void Pause();
        void Seek(long ticks);
        void SkipToNext();
        void SkipToPrevious();
        void SkipTo(int itemIndex);
        void SelectStream(MediaStreamType channel, int index);
        void SetPlaybackSpeed(double speedMultiplier);
    }
}