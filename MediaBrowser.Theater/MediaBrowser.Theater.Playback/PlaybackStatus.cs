using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public struct PlaybackStatus
    {
        public PlayableMedia Media { get; set; }

        public PlaybackStatusType StatusType { get; set; }

        public long Progress { get; set; }

        public decimal Volume { get; set; }

        public bool IsMuted { get; set; }

        public MediaStream[] ActiveStreams { get; set; }
    }
}