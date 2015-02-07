using System.Threading.Tasks;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.MockPlayer
{
    /// <summary>
    ///     The MockMediaPlayer class is an example media player implementation.
    ///     The <see cref="IMediaPlayer" /> implementation acts as the registration and entry point for a media player.
    /// </summary>
    public class MockMediaPlayer : IMediaPlayer
    {
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

        public Task<IPreparedSessions> Prepare(IPlaySequence sequence)
        {
            return Task.FromResult<IPreparedSessions>(new SessionSequence(sequence));
        }
    }
}