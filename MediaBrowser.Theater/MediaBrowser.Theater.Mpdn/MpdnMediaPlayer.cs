using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Mpdn
{
    public class MpdnMediaPlayer : IMediaPlayer
    {
        private readonly ILogManager _logManager;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly IPlaybackManager _playbackManager;

        public int Priority
        {
            get { return 10; }
        }

        public string Name
        {
            get { return "Media Player.NET"; }
        }

        public bool CanPlay(Media media)
        {
            return (media.Item.IsAudio || media.Item.IsVideo) &&
                   media.Item.MediaSources.Any(s => s.Protocol == Model.MediaInfo.MediaProtocol.File && File.Exists(s.Path));
        }

        public bool PrefersBackgroundPlayback
        {
            get { return false; }
        }

        public MpdnMediaPlayer(ILogManager logManager, IWindowManager windowManager, IEventAggregator events, IPlaybackManager playbackManager)
        {
            _logManager = logManager;
            _windowManager = windowManager;
            _events = events;
            _playbackManager = playbackManager;
        }

        public Task<IPreparedSessions> Prepare(IPlaySequence sequence, CancellationToken cancellationToken)
        {
            var sessions = new SessionSequence(sequence, cancellationToken, _windowManager, _events, _logManager.GetLogger("MPDN"), _playbackManager);
            return Task.FromResult<IPreparedSessions>(sessions);
        }
    }
}
