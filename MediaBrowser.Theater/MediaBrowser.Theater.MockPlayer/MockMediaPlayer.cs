using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
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

        public MockMediaPlayer(ILogManager logManager)
        {
            _logManager = logManager;
        }

        public Task<IPreparedSessions> Prepare(IPlaySequence sequence, CancellationToken cancellationToken)
        {
            return Task.FromResult<IPreparedSessions>(new SessionSequence(sequence, cancellationToken, _logManager));
        }
    }
}