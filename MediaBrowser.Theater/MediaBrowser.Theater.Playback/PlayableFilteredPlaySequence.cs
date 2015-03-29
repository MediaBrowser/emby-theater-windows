using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    internal class PlayableFilteredPlaySequence : IPlaySequence<PlayableMedia>
    {
        private readonly IMediaPlayer _player;
        private readonly IPlaySequence<Media> _sequence;

        private PlayableMedia _bootstrap;
        private int? _bootstrapIndex;

        public PlayableFilteredPlaySequence(IPlaySequence<Media> sequence, IMediaPlayer player, PlayableMedia bootstrap = null)
        {
            _sequence = sequence;
            _player = player;
            _bootstrap = bootstrap;
            _bootstrapIndex = bootstrap != null ? (int?) sequence.CurrentIndex : null;
        }

        public void Dispose() { }

        public PlayableMedia Current { get; private set; }

        public int CurrentIndex
        {
            get
            {
                if (_bootstrapIndex != null) {
                    return _bootstrapIndex.Value;
                }

                return _sequence.CurrentIndex;
            }
        }

        public async Task<bool> Next()
        {
            if (_bootstrap != null) {
                Current = _bootstrap;
                _bootstrap = null;
                _bootstrapIndex = null;
                return true;
            }

            bool hasNext = await _sequence.Next();
            if (hasNext) {
                var playable = await _player.GetPlayable(_sequence.Current);
                if (playable != null) {
                    Current = playable;
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public async Task<bool> Previous()
        {
            bool hasPrevious = await _sequence.Previous();
            if (hasPrevious) {
                var playable = await _player.GetPlayable(_sequence.Current);
                if (playable != null) {
                    Current = playable;
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public async Task<bool> SkipTo(int index)
        {
            bool itemExists = await _sequence.SkipTo(index);
            if (itemExists) {
                var playable = await _player.GetPlayable(_sequence.Current);
                if (playable != null) {
                    Current = playable;
                    return true;
                }
            }

            Current = null;
            return false;
        }
    }
}