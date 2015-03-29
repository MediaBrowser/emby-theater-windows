using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
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
        private readonly IPlaySequence<PlayableMedia> _sequence;
        private readonly CancellationToken _cancellationToken;
        private readonly ILogManager _logManager;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly ILogger _log;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;

        private volatile Session _session;

        public SessionSequence(IPlaySequence<PlayableMedia> sequence, CancellationToken cancellationToken, ILogManager logManager, IWindowManager windowManager, IEventAggregator events)
        {
            _id = Interlocked.Increment(ref _counter);
            _sequence = sequence;
            _cancellationToken = cancellationToken;
            _logManager = logManager;
            _windowManager = windowManager;
            _events = events;
            _log = logManager.GetLogger("MockPlayer");
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public async Task<SessionCompletion> Start()
        {
            _log.Debug("Starting session sequence {0} task", _id);

            var state = _windowManager.MainWindowState;
            var window = new MockPlayerWindow {
                Left = (int) (state.Left*state.DpiScale),
                Top = (int) (state.Top*state.DpiScale),
                Width = (int) (state.Width*state.DpiScale),
                Height = (int) (state.Height*state.DpiScale)
            };

            window.GotFocus += (s, e) => _windowManager.FocusMainWindow();

            Action<MainWindowState> updateWindow = s => {
                window.Left = (int) (s.Left*s.DpiScale);
                window.Top = (int) (s.Top*s.DpiScale);
                window.Width = (int) (s.Width*s.DpiScale);
                window.Height = (int) (s.Height*s.DpiScale);
                window.WindowState = GetWindowsFormState(s.State);
            };
            
            window.Show();

            using (Disposable.Create(window.Close))
            using (_windowManager.UseBackgroundWindow(window.Handle))
            using (_events.Get<MainWindowState>().Subscribe(updateWindow)) {
                window.WindowState = GetWindowsFormState(state.State);
                
                var completionState = await RunSessions();
                return completionState;
            }
        }

        private FormWindowState GetWindowsFormState(WindowState state)
        {
            switch (state) {
                case WindowState.Maximized:
                    return FormWindowState.Maximized;
                case WindowState.Minimized:
                    return FormWindowState.Minimized;
            }

            return FormWindowState.Normal;
        }

        private Task<SessionCompletion> RunSessions()
        {
            // run all playback in another task
            return Task.Run(async () => {
                var nextAction = new SessionCompletionAction {
                    Direction = NavigationDirection.Forward
                };

                _log.Debug("Starting session sequence {0} playback", _id);

                // keep moving to the next media until the sequence is complete
                while (await _sequence.MoveNext(nextAction)) {

                    // don't start a new item if cancellation has been requested
                    if (_cancellationToken.IsCancellationRequested) {
                        break;
                    }

                    // create a session for the media
                    _session = new Session(_sequence.Current, _cancellationToken, _logManager);

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
    }
}