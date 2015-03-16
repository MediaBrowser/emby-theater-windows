using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.MockPlayer
{
    /// <summary>
    ///     The MockMediaPlayer class is an example media player implementation.
    ///     The <see cref="IMediaPlayer" /> implementation acts as the registration and entry point for a media player.
    /// </summary>
    public class MockMediaPlayer : IMediaPlayer
    {
        private readonly ILogManager _logManager;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;

        public int Priority
        {
            get { return int.MaxValue; }
        }
        
        public string Name
        {
            get { return "Mock Player"; }
        }

        public bool CanPlay(Media media)
        {
            return true;
        }

        public bool PrefersBackgroundPlayback
        {
            get { return false; }
        }

        public MockMediaPlayer(ILogManager logManager, IWindowManager windowManager, IEventAggregator events)
        {
            _logManager = logManager;
            _windowManager = windowManager;
            _events = events;
        }

        public Task<IPreparedSessions> Prepare(IPlaySequence sequence, CancellationToken cancellationToken)
        {
            return Task.FromResult<IPreparedSessions>(new SessionSequence(sequence, cancellationToken, _logManager, _windowManager, _events));
        }

        public Task Startup()
        {
            return Task.FromResult(0);
        }

        public Task Shutdown()
        {
            return Task.FromResult(0);
        }
    }
}