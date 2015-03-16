using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Async;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;

namespace MediaBrowser.Theater.Playback
{
    public class PlaybackManager : IPlaybackManager
    {
        private static int _count;

        private readonly INavigator _navigator;
        private readonly IConnectionManager _connectionManager;
        private readonly ISubject<PlaybackStatus> _events;
        private readonly ILogger _log;

        private readonly AsyncSemaphore _playbackLock;
        private readonly AsyncSemaphore _playbackStartingLock;
        private readonly List<IMediaPlayer> _players;
        private readonly IPlayQueue _queue;

        private readonly ISubject<IPlaybackSession> _sessions;
        private readonly GlobalPlaybackSettings _settings;
        
        private volatile bool _isPlaying;
        private volatile IPlaybackSession _latestSession;
        private volatile IMediaPlayer _activePlayer;
        private CancellationTokenSource _cancel;

        public PlaybackManager(IEnumerable<IMediaPlayer> players, ILogManager logManager, INavigator navigator, IConnectionManager connectionManager)
        {
            _navigator = navigator;
            _connectionManager = connectionManager;
            _players = new List<IMediaPlayer>(players ?? Enumerable.Empty<IMediaPlayer>());
            _sessions = new Subject<IPlaybackSession>();
            _events = new Subject<PlaybackStatus>();
            _queue = new PlayQueue();
            _settings = new GlobalPlaybackSettings();
            _playbackLock = new AsyncSemaphore(1, 1);
            _playbackStartingLock = new AsyncSemaphore(1, 1);
            _log = logManager.GetLogger(typeof (PlaybackManager).Name);

            _sessions.Subscribe(s => _latestSession = s);
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
        }

        public IObservable<IPlaybackSession> Sessions
        {
            get { return _sessions; }
        }

        public List<IMediaPlayer> Players
        {
            get { return _players; }
        }

        public IPlayQueue Queue
        {
            get { return _queue; }
        }

        public GlobalPlaybackSettings GlobalSettings
        {
            get { return _settings; }
        }

        public IObservable<PlaybackStatus> Events
        {
            get { return _events; }
        }

        public async Task StopPlayback()
        {
            using (await Lock(_playbackStartingLock)) {
                if (_cancel != null) {
                    _cancel.Cancel();
                    _cancel = null;
                }

                using (await Lock(_playbackLock)) { }
            }
        }

        public async Task<IEnumerable<BaseItemDto>> GetIntros(BaseItemDto item)
        {
            if (_connectionManager == null) {
                return Enumerable.Empty<BaseItemDto>();
            }

            var apiClient = _connectionManager.GetApiClient(item);
            var intros = await apiClient.GetIntrosAsync(item.Id, apiClient.CurrentUserId);
            return intros.Items;
        }

        public async Task Initialize()
        {
            await StartInitialPlayer().ConfigureAwait(false);
        }

        public async Task Shutdown()
        {
            await ShutdownActivePlayer().ConfigureAwait(false);
        }

        private async Task StartInitialPlayer()
        {
            using (await Lock(_playbackStartingLock)) {
                var initialPlayer = Players.OrderBy(p => p.Priority).FirstOrDefault();
                if (_activePlayer == null && initialPlayer != null) {
                    await ChangePlayer(initialPlayer);
                }
            }
        }

        private async Task ShutdownActivePlayer()
        {
            using (await Lock(_playbackStartingLock)) {
                await ChangePlayer(null);
            }
        }

        public async Task<bool> AccessSession(Func<IPlaybackSession, Task> action)
        {
            IPlaybackSession session = _latestSession;
            if (session != null) {
                await action(session);
                return true;
            }

            return false;
        }

        public async Task BeginPlayback(int startIndex = 0)
        {
            int id = Interlocked.Increment(ref _count);

            _log.Info("[{1}] Beginning playback at {0}", startIndex, id);

            using (PlayLock playbackLock = await CancelPlaybackAndLock().ConfigureAwait(false)) {
                using (IPlaySequence sequence = Queue.GetPlayOrder()) {
                    try {
                        sequence.SkipTo(startIndex);

                        OnPlaybackStarting();
                        await BeginPlayback(sequence, playbackLock.CancellationTokenSource.Token).ConfigureAwait(false);
                    }
                    finally {
                        _latestSession = null;
                        OnPlaybackFinished();
                    }
                }
            }

            _log.Info("[{0}] Exiting playback", id);
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

        private IMediaPlayer FindSuitablePlayer(Media media)
        {
            return _players.OrderBy(p => p.Priority).FirstOrDefault(p => p.CanPlay(media));
        }

        private async Task<PlayLock> CancelPlaybackAndLock()
        {
            using (await Lock(_playbackStartingLock)) {
                if (_cancel != null) {
                    _cancel.Cancel();
                    _cancel = null;
                }

                await _playbackLock.Wait();

                _cancel = new CancellationTokenSource();
                return new PlayLock(_cancel, _playbackLock);
            }
        }

        private Task BeginPlayback(IPlaySequence sequence, CancellationToken cancellationToken)
        {
            return Task.Run(async () => {
                var firstItem = true;

                Media media;
                IMediaPlayer player;
                while (MoveToNextPlayableItem(sequence, out media, out player)) {
                    if (firstItem && !player.PrefersBackgroundPlayback) {
                        firstItem = false;
                        await _navigator.Navigate(Go.To.FullScreenPlayback()).ConfigureAwait(false);
                    }

                    if (_activePlayer != player) {
                        await ChangePlayer(player).ConfigureAwait(false);
                    }

                    var subSequence = new PlayableFilteredPlaySequence(sequence, player, media);

                    if (cancellationToken.IsCancellationRequested) {
                        break;
                    }

                    IPreparedSessions prepared = await player.Prepare(subSequence, cancellationToken);

                    using (SubscribeToSessions(prepared.Sessions))
                    using (SubscribeToEvents(prepared.Status)) {
                        SessionCompletion status = await prepared.Start().ConfigureAwait(false);
                        if (status == SessionCompletion.Stopped || cancellationToken.IsCancellationRequested) {
                            break;
                        }
                    }
                }
            });
        }

        private async Task ChangePlayer(IMediaPlayer player)
        {
            if (_activePlayer != null) {
                await _activePlayer.Shutdown().ConfigureAwait(false);
            }
            
            _activePlayer = player;

            if (_activePlayer != null) {
                await player.Startup().ConfigureAwait(false);
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

        private bool MoveToNextPlayableItem(IPlaySequence sequence, out Media media, out IMediaPlayer player)
        {
            var searched = new HashSet<Media>();
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

        private async Task<IDisposable> Lock(AsyncSemaphore semaphore)
        {
            await semaphore.Wait();
            return Disposable.Create(semaphore.Release);
        }

        private class PlayLock : IDisposable
        {
            private readonly CancellationTokenSource _cancellationTokenSource;
            private readonly AsyncSemaphore _semaphore;

            public PlayLock(CancellationTokenSource cancellationTokenSource, AsyncSemaphore semaphore)
            {
                _cancellationTokenSource = cancellationTokenSource;
                _semaphore = semaphore;
            }

            public CancellationTokenSource CancellationTokenSource
            {
                get { return _cancellationTokenSource; }
            }

            public void Dispose()
            {
                _semaphore.Release();
            }
        }
    }
}