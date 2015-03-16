using System.Linq;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public static class PlaybackStatusExtensions
    {
        public static int? GetActiveStreamIndex(this PlaybackStatus status, MediaStreamType channel)
        {
            var stream = status.ActiveStreams.FirstOrDefault(s => s.Type == channel);
            if (stream != null) {
                return stream.Index;
            }

            return null;
        }
    }
}