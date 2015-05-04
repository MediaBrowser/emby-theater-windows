using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Mpdn
{
    public class SessionSequence : IPreparedSessions
    {
        private static int _counter = 0;

        private readonly int _id;
        private readonly IPlaySequence<PlayableMedia> _sequence;
        private readonly RemoteClient _api;
        private readonly CancellationToken _cancellationToken;
        private readonly IWindowManager _windowManager;
        private readonly ILogger _log;
        private readonly IPlaybackManager _playbackManager;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;
        
        public SessionSequence(IPlaySequence<PlayableMedia> sequence, RemoteClient api, CancellationToken cancellationToken, IWindowManager windowManager, ILogger log, IPlaybackManager playbackManager)
        {
            _id = Interlocked.Increment(ref _counter);
            _sequence = sequence;
            _api = api;
            _cancellationToken = cancellationToken;
            _windowManager = windowManager;
            _log = log;
            _playbackManager = playbackManager;
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public async Task<SessionCompletion> Start()
        {
            _log.Debug("Starting session sequence");

            var result = await RunSessions(_api).ConfigureAwait(false);
            _log.Debug("Completed sessions");

            return result;
        }

        private Task<SessionCompletion> RunSessions(RemoteClient api)
        {
            // run all playback in another task
            return Task.Run(async () => {
                var nextAction = new SessionCompletionAction {
                    Direction = NavigationDirection.Forward
                };

                _log.Debug("Starting session sequence {0} playback", _id);

                try {
                    // keep moving to the next media until the sequence is complete
                    while (await _sequence.MoveNext(nextAction)) {

                        // don't start a new item if cancellation has been requested
                        if (_cancellationToken.IsCancellationRequested) {
                            break;
                        }

                        _log.Debug("Moving to next item: {0}", _sequence.Current.Media.Item.Path);

                        // create a session for the media
                        var session = new Session(_sequence.Current, api, _cancellationToken, _log, _playbackManager, _windowManager);

                        // forward playback events to our own event observable
                        using (session.Events.Subscribe(status => _status.OnNext(status))) {
                            // notify the playback manager of a new session
                            try {
                                _sessions.OnNext(session);
                            }
                            catch (Exception e) {
                                _log.ErrorException("Error posting next playback session.", e);
                            }

                            // run the session. the session reports which direction to move in to get to the next item
                            nextAction = await session.Run().ConfigureAwait(false);
                        }
                    }

                    _log.Debug("Exiting session sequence {0} playback", _id);
                }
                finally {
                    // notify the playback manager that we have finished, and that there are no more sessions
                    try {
                        _sessions.OnCompleted();
                    }
                    catch (Exception e) {
                        _log.ErrorException("Error closing session observable.", e);
                    }
                }

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
    }
}
