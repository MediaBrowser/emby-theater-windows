using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public static class PlaybackSessionExtensions
    {
        public static void PlayPause(this IPlaybackSession session)
        {
            if (session.Status.StatusType == PlaybackStatusType.Paused) {
                session.Play();
            } else if (session.Status.StatusType == PlaybackStatusType.Playing) {
                session.Pause();
            }
        }

        public static void NextStream(this IPlaybackSession session, MediaStreamType channel)
        {
            List<MediaStream> streams = session.Status.Media.Source.MediaStreams.Where(s => s.Type == channel).ToList();
            if (streams.Count == 0) {
                return;
            }

            int? currentStream = session.Status.GetActiveStreamIndex(channel);
            if (currentStream == null) {
                session.SelectStream(channel, streams.First().Index);
            } else {
                int currentIndex = streams.Select((s, i) => new { s, i })
                                          .Where(item => item.s.Index == currentStream.Value)
                                          .Select(item => item.i)
                                          .First();

                int nextIndex = (currentIndex + 1)%streams.Count;

                if (nextIndex != currentIndex) {
                    session.SelectStream(channel, streams[nextIndex].Index);
                }
            }
        }

        public static void PreviousStream(this IPlaybackSession session, MediaStreamType channel)
        {
            List<MediaStream> streams = session.Status.Media.Source.MediaStreams.Where(s => s.Type == channel).ToList();
            if (streams.Count == 0) {
                return;
            }

            int? currentStream = session.Status.GetActiveStreamIndex(channel);
            if (currentStream == null) {
                session.SelectStream(channel, streams.First().Index);
            } else {
                int currentIndex = streams.Select((s, i) => new { s, i })
                                          .Where(item => item.s.Index == currentStream.Value)
                                          .Select(item => item.i)
                                          .First();

                int nextIndex = currentIndex - 1;
                if (nextIndex < 0) {
                    nextIndex += streams.Count;
                }

                if (nextIndex != currentIndex) {
                    session.SelectStream(channel, streams[nextIndex].Index);
                }
            }
        }

        public static void NextChapter(this IPlaybackSession session)
        {
            PlaybackStatus state = session.Status;
            ChapterInfoDto chapter = state.Media.Media.Item.Chapters.FirstOrDefault(c => c.StartPositionTicks > state.Progress);

            if (chapter != null) {
                session.Seek(chapter.StartPositionTicks);
            }
        }

        public static void PreviousChapter(this IPlaybackSession session)
        {
            PlaybackStatus state = session.Status;
            List<ChapterInfoDto> chapters = state.Media.Media.Item.Chapters;

            for (int i = chapters.Count - 1; i >= 0; i--) {
                ChapterInfoDto previous = chapters[Math.Max(0, i - 1)];
                ChapterInfoDto current = chapters[i];

                if (current.StartPositionTicks < state.Progress) {
                    if (state.Progress - current.StartPositionTicks < TimeSpan.FromSeconds(10).Ticks) {
                        session.Seek(current.StartPositionTicks);
                    } else {
                        session.Seek(previous.StartPositionTicks);
                    }

                    break;
                }
            }
        }

        public static void SkipForward(this IPlaybackSession session, double seconds = 10)
        {
            session.Seek(session.Status.Progress + TimeSpan.FromSeconds(seconds).Ticks);
        }

        public static void SkipBackward(this IPlaybackSession session, double seconds = 10)
        {
            session.Seek(session.Status.Progress - TimeSpan.FromSeconds(seconds).Ticks);
        }
    }
}