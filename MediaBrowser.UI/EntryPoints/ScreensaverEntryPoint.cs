using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Core.Screensaver;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace MediaBrowser.UI.EntryPoints
{
    public class ScreensaverEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly IUserInputManager _userInput;
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playback;
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;

        private DateTime _lastInputTime;
        private Timer _timer;

        public ScreensaverEntryPoint(IUserInputManager userInput, IPresentationManager presentationManager, IPlaybackManager playback, ISessionManager session, IApiClient apiClient, IImageManager imageManager, ILogger logger, IServerEvents serverEvents)
        {
            _userInput = userInput;
            _presentationManager = presentationManager;
            _playback = playback;
            _session = session;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _logger = logger;
            _serverEvents = serverEvents;
        }

        public void Run()
        {
            _playback.PlaybackCompleted += _playback_PlaybackCompleted;
            _playback.PlaybackStarted += _playback_PlaybackStarted;

            _serverEvents.BrowseCommand += _serverEvents_BrowseCommand;
            _serverEvents.MessageCommand += _serverEvents_MessageCommand;
            _serverEvents.PlayCommand += _serverEvents_PlayCommand;
            _serverEvents.PlaystateCommand += _serverEvents_PlaystateCommand;
            _serverEvents.SystemCommand += _serverEvents_SystemCommand;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            StartTimer();
        }

        void _serverEvents_SystemCommand(object sender, SystemCommandEventArgs e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_PlaystateCommand(object sender, PlaystateRequestEventArgs e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_PlayCommand(object sender, PlayRequestEventArgs e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_MessageCommand(object sender, MessageCommandEventArgs e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_BrowseCommand(object sender, BrowseRequestEventArgs e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        private void OnRemoteControlCommand()
        {
            _lastInputTime = DateTime.Now;

            if (_presentationManager.IsScreenSaverRunning)
            {
                _presentationManager.StopScreenSaver();
            }
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            _lastInputTime = DateTime.Now;

            if (_presentationManager.IsScreenSaverRunning)
            {
                _presentationManager.StopScreenSaver();
            }
        }

        void _playback_PlaybackStarted(object sender, PlaybackStartEventArgs e)
        {
            PreventSystemIdle();
        }

        void _playback_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            _lastInputTime = DateTime.Now;

            AllowSystemIdle();
        }

        private void TimerCallback(object state)
        {
            _lastInputTime = new[] { _lastInputTime, _userInput.GetLastInputTime() }.Max();

            if ((DateTime.Now - _lastInputTime) >= TimeSpan.FromSeconds(300))
            {
                ShowScreensaverIfNeeded();
            }
        }

        private void ShowScreensaverIfNeeded()
        {
            var activeMedias = _playback.MediaPlayers
                .Where(i => i.PlayState == PlayState.Playing)
                .Select(i => i.CurrentMedia)
                .Where(i => i != null)
                .ToList();

            if (activeMedias.Any(i => !i.IsAudio))
            {
                _lastInputTime = DateTime.Now;
                return;
            }

            ShowScreensaver();
        }

        private void ShowScreensaver()
        {
            _presentationManager.Window.Dispatcher.Invoke(() =>
            {
                // Don't show screen saver if minimized
                if (_presentationManager.Window.WindowState == WindowState.Minimized)
                {
                    _lastInputTime = DateTime.Now;
                    return;
                }

                StopTimer();

                _logger.Debug("Displaying screen saver");

                new ScreensaverWindow(_session, _apiClient, _imageManager, _logger).ShowModal(_presentationManager.Window);

                StartTimer();
            });
        }

        private void StartTimer()
        {
            _lastInputTime = DateTime.Now;

            if (_timer == null)
            {
                _timer = new Timer(TimerCallback, null, 30000, 30000);
            }
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Dispose()
        {
            _playback.PlaybackCompleted -= _playback_PlaybackCompleted;
            _playback.PlaybackStarted -= _playback_PlaybackStarted;
            
            _serverEvents.BrowseCommand -= _serverEvents_BrowseCommand;
            _serverEvents.MessageCommand -= _serverEvents_MessageCommand;
            _serverEvents.PlayCommand -= _serverEvents_PlayCommand;
            _serverEvents.PlaystateCommand -= _serverEvents_PlaystateCommand;
            _serverEvents.SystemCommand -= _serverEvents_SystemCommand;

            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            
            StopTimer();
        }

        private void PreventSystemIdle()
        {
            _logger.Debug("Calling SetThreadExecutionState to prevent system idle");

            _presentationManager.Window.Dispatcher.InvokeAsync(() =>
            {
                // Prevent system screen saver and monitor power off
                var result = SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);

                if (result == 0)
                {
                    _logger.Warn("SetThreadExecutionState failed");
                }
            });
        }

        private void AllowSystemIdle()
        {
            _logger.Debug("Calling SetThreadExecutionState to allow system idle");

            _presentationManager.Window.Dispatcher.InvokeAsync(() =>
            {
                // Clear EXECUTION_STATE flags to disable away mode and allow the system to idle to sleep normally.
                var result = SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

                if (result == 0)
                {
                    _logger.Warn("SetThreadExecutionState failed");
                }
            });
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
    }

    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

}
