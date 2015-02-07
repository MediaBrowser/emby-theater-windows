using System;

namespace MediaBrowser.Theater.Playback
{
    public static class PlaybackStatusTypeExtensions
    {
        public static bool IsActiveState(this PlaybackStatusType status)
        {
            switch (status) {
                case PlaybackStatusType.Started:
                case PlaybackStatusType.Playing:
                case PlaybackStatusType.Paused:
                    return true;
                case PlaybackStatusType.Complete:
                case PlaybackStatusType.Skipped:
                case PlaybackStatusType.Error:
                case PlaybackStatusType.Stopped:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("status");
            }
        }
    }
}