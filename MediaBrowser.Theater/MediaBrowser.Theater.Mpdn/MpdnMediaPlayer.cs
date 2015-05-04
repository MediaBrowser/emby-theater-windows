using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Mpdn
{
    public class MpdnMediaPlayer : IMediaPlayer
    {
        private const int ApiPort = 6545;

        private readonly ILogManager _logManager;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterApplicationPaths _appPaths;
        private readonly IPlayableMediaBuilder _playableMediaBuilder;

        private IDisposable _player;
        private RemoteClient _api;

        public int Priority
        {
            get { return 10; }
        }

        public string Name
        {
            get { return "Media Player.NET"; }
        }

        public bool CanPlay(Media media)
        {
            return (media.Item.IsAudio || media.Item.IsVideo) &&
                   media.Item.MediaSources.Any(s => s.Protocol == Model.MediaInfo.MediaProtocol.File && File.Exists(s.Path));
        }

        public Task<PlayableMedia> GetPlayable(Media media)
        {
            return _playableMediaBuilder.GetPlayableMedia(media, new MpdnDeviceProfile(), true, CancellationToken.None);
        }

        public bool PrefersBackgroundPlayback
        {
            get { return false; }
        }

        public MpdnMediaPlayer(ILogManager logManager, IWindowManager windowManager, IEventAggregator events, IPlaybackManager playbackManager, ITheaterApplicationPaths appPaths, IPlayableMediaBuilder playableMediaBuilder)
        {
            _logManager = logManager;
            _windowManager = windowManager;
            _events = events;
            _playbackManager = playbackManager;
            _appPaths = appPaths;
            _playableMediaBuilder = playableMediaBuilder;
        }

        public Task<IPreparedSessions> Prepare(IPlaySequence<PlayableMedia> sequence, CancellationToken cancellationToken)
        {
            var sessions = new SessionSequence(sequence, _api, cancellationToken, _windowManager, _logManager.GetLogger("MPDN"), _playbackManager);
            return Task.FromResult<IPreparedSessions>(sessions);
        }

        public async Task Startup()
        {
            var handshake = Task.Run(() => Handshake());
            var process = await StartMpdn().ConfigureAwait(false);

            // wait for startup confirmation from MBT-MPDN extension
            await handshake.ConfigureAwait(false);
            process.WaitForInputIdle();

            var api = await RemoteClient.Connect(new IPEndPoint(IPAddress.Loopback, ApiPort)).ConfigureAwait(false);
            api.Muted += m => _playbackManager.GlobalSettings.Audio.IsMuted = m;
            api.VolumeChanged += v => _playbackManager.GlobalSettings.Audio.Volume = v;

            var background = _windowManager.UseBackgroundWindow(process.MainWindowHandle);

            var window = _windowManager.MainWindowState;
            await MoveWindow(api, window).ConfigureAwait(false);
            var windowMove = _events.Get<MainWindowState>().Subscribe(s => MoveWindow(api, s));

            _api = api;
            _player = Disposable.Create(() => {
                background.Dispose();
                windowMove.Dispose();
                api.Dispose();
                process.CloseMainWindow();
            });
        }

        private static async Task MoveWindow(RemoteClient api, MainWindowState window)
        {
            await api.MoveWindow((int)(window.Left * window.DpiScale),
                                 (int)(window.Top * window.DpiScale),
                                 (int)(window.Width * window.DpiScale),
                                 (int)(window.Height * window.DpiScale),
                                 window.State).ConfigureAwait(false);
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
            var programDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var configDirectory = Path.Combine(_appPaths.PluginsPath, @"MediaPlayerDotNet");

            if (!Directory.Exists(configDirectory)) {
                Directory.CreateDirectory(configDirectory);
            }

            CopyMpdnConfigurationFile(configDirectory, programDirectory);
            CopyRemoteControlConfigurationFile(configDirectory, programDirectory);

            EnsureRemoteClientAuthentication();

            return Task.Run(() => {
                var process = Process.Start(new ProcessStartInfo {
                    FileName = Path.Combine(programDirectory ?? "", @"MPDN\MediaPlayerDotNet.exe"),
                    Arguments = string.Format("--configfolder \"{0}\"", configDirectory),
                    UseShellExecute = false
                });
                
                //process.WaitForInputIdle();
                return process;
            });
        }

        private static void CopyMpdnConfigurationFile(string configDirectory, string programDirectory)
        {
            var configLocation = Path.Combine(configDirectory, "Application.32.config");
            File.Copy(Path.Combine(programDirectory ?? "", @"MPDN\Application.32.config"), configLocation, true);
        }

        private static void CopyRemoteControlConfigurationFile(string configDirectory, string programDirectory)
        {
            var configLocation = Path.Combine(configDirectory, "PlayerExtensions.32", "Example.RemoteSettings.config");
            File.Copy(Path.Combine(programDirectory ?? "", @"MPDN\Example.RemoteSettings.config"), configLocation, true);
        }

        private void EnsureRemoteClientAuthentication()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                              @"MediaPlayerDotNet\RemoteControl");

            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var configLocation = Path.Combine(directory, "accessGUID.conf");

            var guidExistsInFile = File.Exists(configLocation) && File.ReadAllLines(configLocation)
                                                                      .Any(guid => string.Equals(guid.Trim(), RemoteClient.Guid, StringComparison.InvariantCulture));
            
            if (!guidExistsInFile) {
                using (var file = File.Open(configLocation, FileMode.Append))
                using (var writer = new StreamWriter(file)) {
                    writer.WriteLine(RemoteClient.Guid);
                }
            }
        }

        public Task Shutdown()
        {
            if (_player != null) {
                _player.Dispose();
                _player = null;
            }

            return Task.FromResult(0);
        }
    }
}
