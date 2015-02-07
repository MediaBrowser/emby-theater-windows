namespace MediaBrowser.Theater.Playback
{
    internal class PlayableFilteredPlaySequence : IPlaySequence
    {
        private readonly IMediaPlayer _player;
        private readonly IPlaySequence _sequence;

        private Media _bootstrap;

        public PlayableFilteredPlaySequence(IPlaySequence sequence, IMediaPlayer player, Media bootstrap = null)
        {
            _sequence = sequence;
            _player = player;
            _bootstrap = bootstrap;
        }

        public void Dispose()
        {
        }

        public Media Current { get; private set; }

        public bool Next()
        {
            if (_bootstrap != null) {
                Current = _bootstrap;
                _bootstrap = null;
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