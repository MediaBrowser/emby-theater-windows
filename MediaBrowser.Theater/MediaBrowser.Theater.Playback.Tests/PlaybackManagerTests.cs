using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
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
            player.Setup(p => p.GetPlayable(It.IsAny<Media>())).Returns<Media>(m => {
                if (canPlayPredicate(m)) {
                    return Task.FromResult(new PlayableMedia {
                        Media = m
                    });
                }

                return Task.FromResult<PlayableMedia>(null);
            });

            player.Setup(p => p.Prepare(It.IsAny<IPlaySequence<PlayableMedia>>(), It.IsAny<CancellationToken>())).Returns((IPlaySequence<PlayableMedia> sequence, CancellationToken token) =>
            {
                var sessions = new Subject<IPlaybackSession>();
                var events = new Subject<PlaybackStatus>();

                var prepared = new Mock<IPreparedSessions>();
                prepared.Setup(p => p.Sessions).Returns(sessions);
                prepared.Setup(p => p.Status).Returns(events);
                prepared.Setup(p => p.Start()).Returns(() => Task.Run(async () => {
                    while (await sequence.Next()) {
                        var session = new Mock<IPlaybackSession>();
                        sessions.OnNext(session.Object);

                        events.OnNext(new PlaybackStatus {
                            PlayableMedia = sequence.Current,
                            StatusType = PlaybackStatusType.Started
                        });

                        events.OnNext(new PlaybackStatus {
                            PlayableMedia = sequence.Current,
                            StatusType = PlaybackStatusType.Playing
                        });

                        events.OnNext(new PlaybackStatus {
                            PlayableMedia = sequence.Current,
                            StatusType = PlaybackStatusType.Complete
                        });
                    }

                    sessions.OnCompleted();
                    return SessionCompletion.Complete;
                }));

                return Task.FromResult(prepared.Object);
            });

            return player.Object;
        }

        private ILogManager CreateMockLog()
        {
            var logManager = new Mock<ILogManager>();
            logManager.Setup(m => m.GetLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
            return logManager.Object;
        }
        
        [TestMethod]
        public async Task Play()
        {
            var player = CreateMockPlayer(m => true);
            var playbackManager = new PlaybackManager(new[] { player }.ToList(), CreateMockLog(), new Mock<INavigator>().Object, null);

            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);

            var tcs = new TaskCompletionSource<object>();
            var events = new List<PlaybackStatus>();
            playbackManager.Events.Subscribe(e => {
                events.Add(e);

                if (e.PlayableMedia.Media == playbackManager.Queue.Last() && e.StatusType == PlaybackStatusType.Complete) {
                    tcs.SetResult(null);
                }
            });
            
            playbackManager.BeginPlayback();

            await tcs.Task;

            for (int i = 0; i < playbackManager.Queue.Count; i++) {
                events[i*3].Should().Match<PlaybackStatus>(e => e.PlayableMedia.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Started);
                events[i*3 + 1].Should().Match<PlaybackStatus>(e => e.PlayableMedia.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Playing);
                events[i*3 + 2].Should().Match<PlaybackStatus>(e => e.PlayableMedia.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Complete);
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
            var playbackManager = new PlaybackManager(new List<IMediaPlayer> { playerA, playerB }, CreateMockLog(), new Mock<INavigator>().Object, null);

            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);
            playbackManager.Queue.Add(new Mock<BaseItemDto>().Object);

            var tcs = new TaskCompletionSource<object>();
            var events = new List<PlaybackStatus>();
            playbackManager.Events.Subscribe(e => {
                events.Add(e);

                if (e.PlayableMedia.Media == playbackManager.Queue.Last() && e.StatusType == PlaybackStatusType.Complete) {
                    tcs.SetResult(null);
                }
            });

            playbackManager.BeginPlayback();

            await tcs.Task;

            for (int i = 0; i < playbackManager.Queue.Count; i++) {
                events[i*3].Should().Match<PlaybackStatus>(e => e.PlayableMedia.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Started);
                events[i*3 + 1].Should().Match<PlaybackStatus>(e => e.PlayableMedia.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Playing);
                events[i*3 + 2].Should().Match<PlaybackStatus>(e => e.PlayableMedia.Media == playbackManager.Queue[i] && e.StatusType == PlaybackStatusType.Complete);
            }
        }
    }
}
