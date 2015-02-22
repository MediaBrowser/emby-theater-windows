using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
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
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly ILogger _log;
        private readonly IPlaybackManager _playbackManager;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;
        
        public SessionSequence(IPlaySequence sequence, CancellationToken cancellationToken, IWindowManager windowManager, IEventAggregator events, ILogger log, IPlaybackManager playbackManager)
        {
            _id = Interlocked.Increment(ref _counter);
            _sequence = sequence;
            _cancellationToken = cancellationToken;
            _windowManager = windowManager;
            _events = events;
            _log = log;
            _playbackManager = playbackManager;
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public async Task<SessionCompletion> Start()
        {
            _log.Debug("Starting session sequence");

            var handshake = Task.Run(() => Handshake());
            var process = await StartMpdn().ConfigureAwait(false);

            // wait for startup confirmation from MBT-MPDN extension
            await handshake;
            process.WaitForInputIdle();

            using (Disposable.Create(() => process.CloseMainWindow()))
            using (var api = await RemoteClient.Connect(new IPEndPoint(IPAddress.Loopback, ApiPort)).ConfigureAwait(false))
            using (_windowManager.UseBackgroundWindow(process.MainWindowHandle)) {

                api.Muted += m => _playbackManager.GlobalSettings.Audio.IsMuted = m;
                api.VolumeChanged += v => _playbackManager.GlobalSettings.Audio.Volume = v;
                
                var window = _windowManager.MainWindowState;
                await MoveWindow(api, window).ConfigureAwait(false);

                using (_events.Get<MainWindowState>().Subscribe(s => MoveWindow(api, s))) {
                    var result = await RunSessions(api).ConfigureAwait(false);
                    _log.Debug("Completed sessions");
                    return result;
                }
            }
        }

        private static async Task MoveWindow(RemoteClient api, MainWindowState window)
        {
            await api.MoveWindow((int) (window.Left*window.DpiScale),
                                 (int) (window.Top*window.DpiScale),
                                 (int) (window.Width*window.DpiScale),
                                 (int) (window.Height*window.DpiScale),
                                 window.State);
        }

        private async Task Handshake()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 6546);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(2);

            var clientSocket = serverSocket.Accept();
            using (var stream = new NetworkStream(clientSocket))
            using (var writer = new StreamWriter(stream) { AutoFlush = true })
            using (var reader = new StreamReader(stream)) {
                await reader.ReadLineAsync().ConfigureAwait(false);
                await writer.WriteLineAsync("OK").ConfigureAwait(false);
            }
        }

        private Task<Process> StartMpdn()
        {
            return Task.Run(() => {
                var process = Process.Start(new ProcessStartInfo {
                    FileName = @"D:\Projects\MPDN\MediaPlayerDotNet.exe"
                });
                
                //process.WaitForInputIdle();
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

                        _log.Debug("Moving to next item: {0}", _sequence.Current.Item.Path);

                        // create a session for the media
                        PlayableMedia item = GetPlayableMedia(_sequence.Current);
                        var session = new Session(item, api, _cancellationToken, _log, _playbackManager);

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
            var source = media.Item.MediaSources.FirstOrDefault(s => s.Protocol == Model.MediaInfo.MediaProtocol.File && File.Exists(s.Path));
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
