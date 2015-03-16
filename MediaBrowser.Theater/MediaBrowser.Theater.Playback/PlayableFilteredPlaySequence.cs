namespace MediaBrowser.Theater.Playback
{
    internal class PlayableFilteredPlaySequence : IPlaySequence
    {
        private readonly IMediaPlayer _player;
        private readonly IPlaySequence _sequence;

        private Media _bootstrap;
        private int? _bootstrapIndex;

        public PlayableFilteredPlaySequence(IPlaySequence sequence, IMediaPlayer player, Media bootstrap = null)
        {
            _sequence = sequence;
            _player = player;
            _bootstrap = bootstrap;
            _bootstrapIndex = bootstrap != null ? (int?)sequence.CurrentIndex : null;
        }

        public void Dispose()
        {
        }

        public Media Current { get; private set; }

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

        public bool Next()
        {
            if (_bootstrap != null) {
                Current = _bootstrap;
                _bootstrap = null;
                _bootstrapIndex = null;
                return true;
            }

            bool hasNext = _sequence.Next();
            if (hasNext) {
                bool canPlay = _player.CanPlay(_sequence.Current);

                if (canPlay) {
                    Current = _sequence.Current;
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public bool Previous()
        {
            bool hasPrevious = _sequence.Previous();
            if (hasPrevious) {
                bool canPlay = _player.CanPlay(_sequence.Current);

                if (canPlay) {
                    Current = _sequence.Current;
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public bool SkipTo(int index)
        {
            bool itemExists = _sequence.SkipTo(index);
            if (itemExists) {
                bool canPlay = _player.CanPlay(_sequence.Current);

                if (canPlay) {
                    Current = _sequence.Current;
                    return true;
                }
            }

            Current = null;
            return false;
        }
    }
}