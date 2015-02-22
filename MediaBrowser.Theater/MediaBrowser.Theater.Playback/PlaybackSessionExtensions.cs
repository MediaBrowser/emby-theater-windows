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
            List<MediaStream> streams = session.Status.PlayableMedia.Source.MediaStreams.Where(s => s.Type == channel).ToList();
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
            List<MediaStream> streams = session.Status.PlayableMedia.Source.MediaStreams.Where(s => s.Type == channel).ToList();
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
            List<ChapterInfoDto> chapters = state.PlayableMedia.Media.Item.Chapters;
            ChapterInfoDto chapter = chapters != null ? chapters.FirstOrDefault(c => c.StartPositionTicks > state.Progress) : null;

            if (chapter != null) {
                session.Seek(chapter.StartPositionTicks);
            } else {
                PlaybackStatus status = session.Status;
                TimeSpan skip = SkipDuration(status);
                session.Seek((status.Progress ?? 0) + skip.Ticks);
            }
        }

        public static void PreviousChapter(this IPlaybackSession session)
        {
            PlaybackStatus state = session.Status;
            List<ChapterInfoDto> chapters = state.PlayableMedia.Media.Item.Chapters;

            if (chapters != null && chapters.Count > 0) {
                for (int i = chapters.Count - 1; i >= 0; i--) {
                    ChapterInfoDto previous = chapters[Math.Max(0, i - 1)];
                    ChapterInfoDto current = chapters[i];

                    if (current.StartPositionTicks >= state.Progress) {
                        continue;
                    }

                    if (state.Progress - current.StartPositionTicks > TimeSpan.FromSeconds(10).Ticks) {
                        session.Seek(current.StartPositionTicks);
                    } else {
                        session.Seek(previous.StartPositionTicks);
                    }

                    break;
                }
            } else {
                PlaybackStatus status = session.Status;
                TimeSpan skip = SkipDuration(status);
                session.Seek((status.Progress ?? 0) - skip.Ticks);
            }
        }

        private static TimeSpan SkipDuration(PlaybackStatus status)
        {
            if (status.Duration > TimeSpan.FromMinutes(20).Ticks) {
                return TimeSpan.FromMinutes(5);
            }

            if (status.Duration > TimeSpan.FromMinutes(10).Ticks) {
                return TimeSpan.FromMinutes(2);
            }

            if (status.Duration > TimeSpan.FromMinutes(2).Ticks) {
                return TimeSpan.FromSeconds(30);
            }

            return TimeSpan.FromSeconds(5);
        }

        public static void SkipForward(this IPlaybackSession session, double seconds = 10)
        {
            session.Seek(session.Status.Progress ?? 0 + TimeSpan.FromSeconds(seconds).Ticks);
        }

        public static void SkipBackward(this IPlaybackSession session, double seconds = 10)
        {
            session.Seek(session.Status.Progress ?? 0 - TimeSpan.FromSeconds(seconds).Ticks);
        }

        public static void FastForward(this IPlaybackSession session)
        {
            double currentSpeed = session.Status.Speed;

            if (currentSpeed > 0) {
                session.SetPlaybackSpeed(Math.Max(currentSpeed*2, 16));
            } else {
                if (currentSpeed >= -1.5) {
                    session.SetPlaybackSpeed(1);
                } else {
                    session.SetPlaybackSpeed(currentSpeed*0.5);
                }
            }
        }

        public static void Rewind(this IPlaybackSession session)
        {
            double currentSpeed = session.Status.Speed;

            if (currentSpeed < 0) {
                session.SetPlaybackSpeed(Math.Max(currentSpeed*2, 16));
            } else {
                if (currentSpeed <= 1.5) {
                    session.SetPlaybackSpeed(-1);
                } else {
                    session.SetPlaybackSpeed(currentSpeed*0.5);
                }
            }
        }
    }
}