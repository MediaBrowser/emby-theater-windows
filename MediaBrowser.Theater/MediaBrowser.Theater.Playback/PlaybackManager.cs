using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Async;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public class PlaybackManager : IPlaybackManager
    {
        private readonly ISubject<PlaybackStatus> _events;
        private readonly IList<IMediaPlayer> _players;
        private readonly IPlayQueue _queue;
        private readonly AsyncSemaphore _sessionLock;

        private readonly ISubject<IPlaybackSession> _sessions;

        private volatile bool _isPlaying;
        private volatile IPlaybackSession _latestSession;

        public PlaybackManager(IList<IMediaPlayer> players)
        {
            _players = players;
            _sessions = new Subject<IPlaybackSession>();
            _events = new Subject<PlaybackStatus>();
            _queue = new PlayQueue();
            _sessionLock = new AsyncSemaphore(1, 1);

            _sessions.Subscribe(s => _latestSession = s);
            _sessions.OnNext(new NullSession(this));
        }

        public IObservable<IPlaybackSession> Sessions
        {
            get { return _sessions; }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
        }

        public IPlayQueue Queue
        {
            get { return _queue; }
        }

        public Task<IPlaybackSessionAccessor> GetSessionLock()
        {
            return LockedPlaybackSessionAccessor.Wrap(_latestSession, _sessionLock);
        }

        public IObservable<PlaybackStatus> Events
        {
            get { return _events; }
        }

        public event Action PlaybackStarting;

        protected virtual void OnPlaybackStarting()
        {
            _isPlaying = true;

            Action handler = PlaybackStarting;
            if (handler != null) {
                handler();
            }
        }

        public event Action PlaybackFinished;

        protected virtual void OnPlaybackFinished()
        {
            _isPlaying = false;

            Action handler = PlaybackFinished;
            if (handler != null) {
                handler();
            }
        }

        private IMediaPlayer FindSuitablePlayer(MediaStreamType media)
        {
            return _players.FirstOrDefault(p => p.CanPlay(media));
        }

        private async void Start()
        {
            using (IPlaySequence sequence = Queue.GetPlayOrder()) {
                try {
                    OnPlaybackStarting();
                    await BeginPlayback(sequence);
                }
                finally {
                    _sessions.OnNext(new NullSession(this));
                    OnPlaybackFinished();
                }
            }
        }

        private async Task BeginPlayback(IPlaySequence sequence)
        {
            BaseItemDto media;
            IMediaPlayer player;
            while (MoveToNextPlayableItem(sequence, out media, out player)) {
                var subSequence = new PlayableFilteredPlaySequence(sequence, player, media);
                IPreparedSessions prepared = await player.Prepare(subSequence);

                using (SubscribeToSessions(prepared.Sessions)) {
                    using (SubscribeToEvents(prepared.Status)) {
                        await prepared.Start();

                        IPlaybackSession finalSession = await prepared.Sessions.DefaultIfEmpty();
                        bool shouldContinue = !WasStopped(finalSession);

                        if (!shouldContinue) {
                            break;
                        }
                    }
                }
            }
        }

        private IDisposable SubscribeToSessions(IObservable<IPlaybackSession> sessions)
        {
            return sessions.Subscribe(session => _sessions.OnNext(session));
        }

        private IDisposable SubscribeToEvents(IObservable<PlaybackStatus> status)
        {
            return status.Subscribe(e => _events.OnNext(e));
        }

        private bool WasStopped(IPlaybackSession session)
        {
            if (session == null) {
                return false;
            }

            return session.Status.StatusType == PlaybackStatusType.Stopped;
        }

        private bool MoveToNextPlayableItem(IPlaySequence sequence, out BaseItemDto media, out IMediaPlayer player)
        {
            var searched = new HashSet<BaseItemDto>();
            do {
                if (sequence.Current == null) {
                    continue;
                }

                player = FindSuitablePlayer(sequence.Current);
                if (player != null) {
                    media = sequence.Current;
                    return true;
                }

                searched.Add(sequence.Current);
                if (searched.Count >= Queue.Count && Queue.All(searched.Contains)) {
                    break;
                }
            } while (sequence.Next());

            media = null;
            player = null;
            return false;
        }

        #region NullSession

        private class NullSession : IPlaybackSession
        {
            private readonly PlaybackManager _playbackManager;

            public NullSession(PlaybackManager playbackManager)
            {
                _playbackManager = playbackManager;
            }

            public void Play()
            {
                if (Capabilities.CanPlay) {
                    _playbackManager.Start();
                }
            }

            public void Pause()
            {
            }

            public Task Stop()
            {
                return Task.FromResult(0);
            }

            public void Seek(long ticks)
            {
            }

            public void SkipNext()
            {
            }

            public void SkipPrevious()
            {
            }

            public void SelectStream(MediaStreamType channel, int index)
            {
            }

            public void SetVolume(decimal volume)
            {
            }

            public void SetMuted(bool muted)
            {
            }

            public IObservable<PlaybackStatus> Events { get; private set; }

            public PlaybackCapabilities Capabilities
            {
                get
                {
                    return new PlaybackCapabilities {
                        CanPlay = !_playbackManager.IsPlaying && _playbackManager.Queue.Count > 0
                    };
                }
            }

            public PlaybackStatus Status { get; private set; }
        }

        #endregion
    }

    internal class PlayableFilteredPlaySequence : IPlaySequence
    {
        private readonly IMediaPlayer _player;
        private readonly IPlaySequence _sequence;

        private BaseItemDto _bootstrap;

        public PlayableFilteredPlaySequence(IPlaySequence sequence, IMediaPlayer player, BaseItemDto bootstrap = null)
        {
            _sequence = sequence;
            _player = player;
            _bootstrap = bootstrap;
        }

        public void Dispose()
        {
        }

        public BaseItemDto Current { get; private set; }

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
    }
}