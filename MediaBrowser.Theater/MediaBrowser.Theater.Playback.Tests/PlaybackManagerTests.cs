using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Model.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MediaBrowser.Theater.Playback.Tests
{
    [TestClass]
    public class PlaybackManagerTests
    {
        private IMediaPlayer CreateMockPlayer(Func<Media, bool> canPlayPredicate)
        {
            var player = new Mock<IMediaPlayer>();
            player.Setup(p => p.CanPlay(It.IsAny<Media>())).Returns(canPlayPredicate);

            player.Setup(p => p.Prepare(It.IsAny<IPlaySequence>())).Returns((IPlaySequence sequence) =>
            {
                var sessions = new Subject<IPlaybackSession>();
                var events = new Subject<PlaybackStatus>();

                var prepared = new Mock<IPreparedSessions>();
                prepared.Setup(p => p.Sessions).Returns(sessions);
                prepared.Setup(p => p.Status).Returns(events);
                prepared.Setup(p => p.Start()).Returns(() => Task.Run(() => {
                    while (sequence.Next()) {
                        var session = new Mock<IPlaybackSession>();
                        sessions.OnNext(session.Object);

                        events.OnNext(new PlaybackStatus {
                            Media = new PlayableMedia { Media = sequence.Current },
                            StatusType = PlaybackStatusType.Started
                        });

                        events.OnNext(new PlaybackStatus {
                            Media = new PlayableMedia { Media = sequence.Current },
                            StatusType = PlaybackStatusType.Playing
                        });

                        events.OnNext(new PlaybackStatus {
                            Media = new PlayableMedia { Media = sequence.Current },
                            StatusType = PlaybackStatusType.Complete
                        });
                    }

                    sessions.OnCompleted();
                }));

                return Task.FromResult(prepared.Object);
            });

            return player.Object;
        }

        [TestMethod]
        public async Task Play()
        {
            var player = CreateMockPlayer(m => true);
            var playbackManager = new PlaybackManager(new[] {player}.ToList());

            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);

            var tcs = new TaskCompletionSource<object>();
            var events = new List<PlaybackStatus>();
            playbackManager.Events.Subscribe(e => {
                events.Add(e);

                if (e.Media.Media == playbackManager.Queue.Last() && e.StatusType == PlaybackStatusType.Complete) {
                    tcs.SetResult(null);
                }
            });

            using (var accessor = await playbackManager.GetSessionLock()) {
                accessor.Session.Play();
            }

            await tcs.Task;

            for (int i = 0; i < playbackManager.Queue.Count; i++) {
                events[i*3].Should().Match<PlaybackStatus>(e => e.Media.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Started);
                events[i*3 + 1].Should().Match<PlaybackStatus>(e => e.Media.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Playing);
                events[i*3 + 2].Should().Match<PlaybackStatus>(e => e.Media.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Complete);
            }
        }

        [TestMethod]
        public async Task Play_SwitchPlayer()
        {
            var media = new Media[] {
                new Mock<BaseItemDto>().Object,
                new Mock<BaseItemDto>().Object,
                new Mock<BaseItemDto>().Object,
                new Mock<BaseItemDto>().Object,
                new Mock<BaseItemDto>().Object,
            };

            var playerA = CreateMockPlayer(m => m == media[2]);
            var playerB = CreateMockPlayer(m => m != media[2]);
            var playbackManager = new PlaybackManager(new List<IMediaPlayer> {playerA, playerB});

            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);

            var tcs = new TaskCompletionSource<object>();
            var events = new List<PlaybackStatus>();
            playbackManager.Events.Subscribe(e => {
                events.Add(e);

                if (e.Media.Media == playbackManager.Queue.Last() && e.StatusType == PlaybackStatusType.Complete) {
                    tcs.SetResult(null);
                }
            });

            using (var accessor = await playbackManager.GetSessionLock()) {
                accessor.Session.Play();
            }

            await tcs.Task;

            for (int i = 0; i < playbackManager.Queue.Count; i++) {
                events[i*3].Should().Match<PlaybackStatus>(e => e.Media.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Started);
                events[i*3 + 1].Should().Match<PlaybackStatus>(e => e.Media.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Playing);
                events[i*3 + 2].Should().Match<PlaybackStatus>(e => e.Media.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Complete);
            }
        }
    }
}
