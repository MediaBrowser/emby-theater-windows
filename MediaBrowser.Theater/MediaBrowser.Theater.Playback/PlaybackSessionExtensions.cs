using System;
using System.Linq;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public static class PlaybackSessionExtensions
    {
        public static void PlayPause(this IPlaybackSession session)
        {
            if (session.Status.StatusType == PlaybackStatusType.Paused) {
                session.Play();
            }
            else if (session.Status.StatusType == PlaybackStatusType.Playing) {
                session.Pause();
            }
        }

        public static void NextStream(this IPlaybackSession session, MediaStreamType channel)
        {
            var streams = session.Status.Media.Source.MediaStreams.Where(s => s.Type == channel).ToList();
            if (streams.Count == 0) {
                return;
            }
            
            var currentStream = session.Status.GetActiveStreamIndex(channel);
            if (currentStream == null) {
                session.SelectStream(channel, streams.First().Index);
            }
            else {
                var currentIndex = streams.Select((s, i) => new {s, i})
                    .Where(item => item.s.Index == currentStream.Value)
                    .Select(item => item.i)
                    .First();

                var nextIndex = (currentIndex + 1)%streams.Count;

                if (nextIndex != currentIndex) {
                    session.SelectStream(channel, streams[nextIndex].Index);
                }
            }
        }

        public static void PreviousStream(this IPlaybackSession session, MediaStreamType channel)
        {
            var streams = session.Status.Media.Source.MediaStreams.Where(s => s.Type == channel).ToList();
            if (streams.Count == 0) {
                return;
            }

            var currentStream = session.Status.GetActiveStreamIndex(channel);
            if (currentStream == null) {
                session.SelectStream(channel, streams.First().Index);
            } else {
                var currentIndex = streams.Select((s, i) => new { s, i })
                    .Where(item => item.s.Index == currentStream.Value)
                    .Select(item => item.i)
                    .First();

                var nextIndex = currentIndex - 1;
                if (nextIndex < 0) {
                    nextIndex += streams.Count;
                }

                if (nextIndex != currentIndex) {
                    session.SelectStream(channel, streams[nextIndex].Index);
                }
            }
        }

        public static void StepUpVolume(this IPlaybackSession session)
        {
            if (session.Status.IsMuted) {
                session.SetMuted(false);
            }

            var volume = session.Status.Volume;
            volume = Math.Min(Math.Max(volume + 0.02m, 0), 1);

            session.SetVolume(volume);
        }

        public static void StepDownVolume(this IPlaybackSession session)
        {
            if (session.Status.IsMuted)
            {
                session.SetMuted(false);
            }

            var volume = session.Status.Volume;
            volume = Math.Min(Math.Max(volume - 0.02m, 0), 1);

            session.SetVolume(volume);
        }

        // todo session extension for seeking to chapter
    }
}