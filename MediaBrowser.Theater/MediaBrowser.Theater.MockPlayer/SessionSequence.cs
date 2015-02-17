using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.MockPlayer
{
    /// <summary>
    ///     The SessionSequence class is an example implementation of <see cref="IPreparedSessions" />.
    ///     The class is responsible for managing the sessions in a play sequence.
    /// </summary>
    internal class SessionSequence : IPreparedSessions
    {
        private static int _counter = 0;

        private readonly int _id;
        private readonly IPlaySequence _sequence;
        private readonly CancellationToken _cancellationToken;
        private readonly ILogManager _logManager;
        private readonly ILogger _log;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;

        private volatile Session _session;

        public SessionSequence(IPlaySequence sequence, CancellationToken cancellationToken, ILogManager logManager)
        {
            _id = Interlocked.Increment(ref _counter);
            _sequence = sequence;
            _cancellationToken = cancellationToken;
            _logManager = logManager;
            _log = logManager.GetLogger("MockPlayer");
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public Task<SessionCompletion> Start()
        {
            _log.Debug("Starting session sequence {0} task", _id);

            // run all playback in another task
            return Task.Run(async () => {
                var nextAction = new SessionCompletionAction {
                    Direction = NavigationDirection.Forward
                };

                _log.Debug("Starting session sequence {0} playback", _id);

                // keep moving to the next media until the sequence is complete
                while (MoveNext(nextAction)) {

                    // don't start a new item if cancellation has been requested
                    if (_cancellationToken.IsCancellationRequested) {
                        break;
                    }

                    // create a session for the media
                    PlayableMedia item = GetPlayableMedia(_sequence.Current);
                    _session = new Session(item, _cancellationToken, _logManager);

                    // forward playback events to our own event observable
                    using (_session.Events.Subscribe(status => _status.OnNext(status))) {
                        // notify the playback manager of a new session
                        _sessions.OnNext(_session);

                        // run the session. the session reports which direction to move in to get to the next item
                        nextAction = await _session.Run();
                    }
                }

                _log.Debug("Exiting session sequence {0} playback", _id);

                // notify the playback manager that we have finished, and that there are no more sessions
                _sessions.OnCompleted();

                return _cancellationToken.IsCancellationRequested ? SessionCompletion.Stopped : SessionCompletion.Complete;
            });
        }
        
        public IObservable<IPlaybackSession> Sessions
        {
            get { return _sessions; }
        }

        public IObservable<PlaybackStatus> Status
        {
            get { return _status; }
        }

        private bool MoveNext(SessionCompletionAction action)
        {
            switch (action.Direction) {
                case NavigationDirection.Forward:
                    return _sequence.Next();
                case NavigationDirection.Backward:
                    return _sequence.Previous();
                case NavigationDirection.Skip:
                    return _sequence.SkipTo(action.Index);
            }

            return false;
        }

        private PlayableMedia GetPlayableMedia(Media media)
        {
            return new PlayableMedia {
                Media = media,
                Source = media.Item.MediaSources.First()
            };
        }
    }
}