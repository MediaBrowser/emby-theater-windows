using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MediaBrowser.Theater.Playback.Tests
{
    [TestClass]
    public class PlaybackSessionTests
    {
        [TestMethod]
        public void NextStream()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0]},
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 1)).Verifiable();

            session.Object.NextStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void NextStream_NoneSelected()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new IMediaStream[0],
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 0)).Verifiable();

            session.Object.NextStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void NextStream_Wrap()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[2]},
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 0)).Verifiable();

            session.Object.NextStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void NextStream_MixedTypes()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Video, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Subtitle, Index = 0}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0], streams[1], streams[3]},
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 1)).Verifiable();

            session.Object.NextStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[1]},
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 0)).Verifiable();

            session.Object.PreviousStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream_NoneSelected()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new IMediaStream[0],
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 0)).Verifiable();

            session.Object.PreviousStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream_Wrap()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0]},
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 2)).Verifiable();

            session.Object.PreviousStream(StreamChannel.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream_MixedTypes()
        {
            var streams = new IMediaStream[] {
                new MediaStream {Channel = StreamChannel.Audio, Index = 0},
                new MediaStream {Channel = StreamChannel.Video, Index = 0},
                new MediaStream {Channel = StreamChannel.Audio, Index = 1},
                new MediaStream {Channel = StreamChannel.Subtitle, Index = 0}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0], streams[1], streams[3]},
                Media = new PlayableMedia {
                    Streams = streams
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<StreamChannel>(c => c == StreamChannel.Audio), 1)).Verifiable();

            session.Object.PreviousStream(StreamChannel.Audio);

            session.Verify();
        }
    }
}