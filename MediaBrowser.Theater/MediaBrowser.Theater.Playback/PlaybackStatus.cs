using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public struct PlaybackStatus
    {
        public PlayableMedia Media { get; set; }

        public PlaybackStatusType StatusType { get; set; }

        public long Progress { get; set; }

        public long Duration { get; set; }

        public double Speed { get; set; }

        public MediaStream[] ActiveStreams { get; set; }
    }
}