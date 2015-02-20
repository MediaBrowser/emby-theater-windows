using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
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
        private const int ApiPort = 6545;
        private static int _counter = 0;

        private readonly int _id;
        private readonly IPlaySequence _sequence;
        private readonly CancellationToken _cancellationToken;
        private readonly ILogManager _logManager;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly ILogger _log;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;
        
        public SessionSequence(IPlaySequence sequence, CancellationToken cancellationToken, ILogManager logManager, IWindowManager windowManager, IEventAggregator events, ILogger log)
        {
            _id = Interlocked.Increment(ref _counter);
            _sequence = sequence;
            _cancellationToken = cancellationToken;
            _logManager = logManager;
            _windowManager = windowManager;
            _events = events;
            _log = log;
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public async Task<SessionCompletion> Start()
        {
            _log.Debug("Starting session sequence");
            
            var process = await StartMpdn();

            using (Disposable.Create(() => process.CloseMainWindow()))
            using (var api = await RemoteClient.Connect(new IPEndPoint(IPAddress.Loopback, ApiPort)))
            using (_windowManager.UseBackgroundWindow(process.MainWindowHandle)) {

                // todo position mpdn window, set volume/mute on setting change

                return await RunSessions(api);
            }
        }

        private Task<Process> StartMpdn()
        {
            return Task.Run(() => {
                var process = Process.Start(new ProcessStartInfo {
                    FileName = @"D:\Projects\MPDN\MediaPlayerDotNet.exe"
                });

                process.WaitForInputIdle();
                return process;
            });
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
                    while (MoveNext(nextAction)) {

                        // don't start a new item if cancellation has been requested
                        if (_cancellationToken.IsCancellationRequested) {
                            break;
                        }

                        // create a session for the media
                        PlayableMedia item = GetPlayableMedia(_sequence.Current);
                        var session = new Session(item, api, _cancellationToken, _logManager);

                        // forward playback events to our own event observable
                        using (session.Events.Subscribe(status => _status.OnNext(status))) {
                            // notify the playback manager of a new session
                            _sessions.OnNext(session);

                            // run the session. the session reports which direction to move in to get to the next item
                            nextAction = await session.Run();
                        }
                    }

                    _log.Debug("Exiting session sequence {0} playback", _id);
                    
                    return _cancellationToken.IsCancellationRequested ? SessionCompletion.Stopped : SessionCompletion.Complete;
                }
                finally {
                    // notify the playback manager that we have finished, and that there are no more sessions
                    _sessions.OnCompleted();
                }
            });
        }

        private bool MoveNext(SessionCompletionAction action)
        {
            switch (action.Direction)
            {
                case NavigationDirection.Forward:
                    return _sequence.Next();
                case NavigationDirection.Backward:
                    return _sequence.Previous();
                case NavigationDirection.Skip:
                    return _sequence.SkipTo(action.Index);
            }

            return false;
        }

        public IObservable<IPlaybackSession> Sessions
        {
            get { return _sessions; }
        }

        public IObservable<PlaybackStatus> Status
        {
            get { return _status; }
        }

        private PlayableMedia GetPlayableMedia(Media media)
        {
            var source = media.Item.MediaSources.First(s => s.Protocol == Model.MediaInfo.MediaProtocol.File && File.Exists(s.Path));
            if (source == null) {
                throw new InvalidOperationException(string.Format("MPDN cannot play {0}, as it has no file accessible source.", media.Item.Name));
            }
            
            return new PlayableMedia
            {
                Media = media,
                Source = source,
                Path = source.Path
            };
        }
    }
}
