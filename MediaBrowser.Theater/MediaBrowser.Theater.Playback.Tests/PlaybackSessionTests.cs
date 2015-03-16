using System.Collections.Generic;
using MediaBrowser.Model.Entities;
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
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0]},
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 1)).Verifiable();

            session.Object.NextStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void NextStream_NoneSelected()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new MediaStream[0],
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 0)).Verifiable();

            session.Object.NextStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void NextStream_Wrap()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[2]},
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 0)).Verifiable();

            session.Object.NextStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void NextStream_MixedTypes()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Video, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Subtitle, Index = 0}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0], streams[1], streams[3]},
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 1)).Verifiable();

            session.Object.NextStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[1]},
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 0)).Verifiable();

            session.Object.PreviousStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream_NoneSelected()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new MediaStream[0],
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 0)).Verifiable();

            session.Object.PreviousStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream_Wrap()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Audio, Index = 2}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0]},
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 2)).Verifiable();

            session.Object.PreviousStream(MediaStreamType.Audio);

            session.Verify();
        }

        [TestMethod]
        public void PreviousStream_MixedTypes()
        {
            var streams = new List<MediaStream> {
                new MediaStream {Type = MediaStreamType.Audio, Index = 0},
                new MediaStream {Type = MediaStreamType.Video, Index = 0},
                new MediaStream {Type = MediaStreamType.Audio, Index = 1},
                new MediaStream {Type = MediaStreamType.Subtitle, Index = 0}
            };

            var status = new PlaybackStatus {
                ActiveStreams = new[] {streams[0], streams[1], streams[3]},
                PlayableMedia = new PlayableMedia {
                    Source = new Model.Dto.MediaSourceInfo { MediaStreams = streams }
                }
            };

            var session = new Mock<IPlaybackSession>();
            session.Setup(s => s.Status).Returns(status);
            session.Setup(s => s.SelectStream(It.Is<MediaStreamType>(c => c == MediaStreamType.Audio), 1)).Verifiable();

            session.Object.PreviousStream(MediaStreamType.Audio);

            session.Verify();
        }
    }
}