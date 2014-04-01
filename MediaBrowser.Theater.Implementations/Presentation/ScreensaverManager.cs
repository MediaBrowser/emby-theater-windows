using System.Collections.Generic;
using System.Windows.Media;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Implementations.Session;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Implementations.Presentation
{
    public class ScreensaverManager : IScreensaverManager, IDisposable
    {
        private const int ScreensaverIdleTimeoutSecs = 300; // timeout if we have been idle for 5 minutes - use 15 for debug
        private const int ScreensaverStartCheckMSec = 30000; // check idle timeout every 30 seconnds - use 1000 for debug

        private readonly IUserInputManager _userInput;
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playback;
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly ITheaterConfigurationManager _theaterConfigurationManager;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;

        private DateTime _lastInputTime;
        private Timer _timer;

        public ScreensaverManager(IUserInputManager userInput, IPresentationManager presentationManager, IPlaybackManager playback, ISessionManager session, IApiClient apiClient, ITheaterConfigurationManager theaterConfigurationManager, ILogManager logManager, IServerEvents serverEvents)
        {
            _userInput = userInput;
            _presentationManager = presentationManager;
            _playback = playback;
            _session = session;
            _apiClient = apiClient;
            _theaterConfigurationManager = theaterConfigurationManager;
            _logger = logManager.GetLogger(GetType().Name);
            _serverEvents = serverEvents;

            _session.UserLoggedIn += session_UserChanged;
            _session.UserLoggedOut += session_UserChanged;

            _playback.PlaybackCompleted += _playback_PlaybackCompleted;
            _playback.PlaybackStarted += _playback_PlaybackStarted;

            _serverEvents.BrowseCommand += _serverEvents_BrowseCommand;
            _serverEvents.MessageCommand += _serverEvents_MessageCommand;
            _serverEvents.PlayCommand += _serverEvents_PlayCommand;
            _serverEvents.PlaystateCommand += _serverEvents_PlaystateCommand;

            _serverEvents.GeneralCommand += _serverEvents_GeneralCommand; 

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
          

            StartTimer();
        }

        private void SetDefaultCurrentScreenSaverName()
        {
            if (_session.CurrentUser != null)
            {
                var conf = _theaterConfigurationManager.GetUserTheaterConfiguration(_session.CurrentUser.Id);
                CurrentScreensaverName = conf != null ? conf.Screensaver : "Backdrop";
            }
            else
            {
                CurrentScreensaverName = "Logo";
            }
        }

        private void session_UserChanged(object sender, EventArgs e)
        {
            SetDefaultCurrentScreenSaverName();
        }

        /// <summary>
        /// Gets/Set the current screen save factory list
        /// </summary>
        /// <value>The c current screen save factory list.</value>
        public IEnumerable<IScreensaverFactory> ScreensaverFactories { get; private set; }

        /// <summary>
        /// Gets/Set the current selected screen saver 
        /// </summary>
        /// <value>The current selected screen saver.</value>
        public string CurrentScreensaverName { get; set; }

        public void AddParts(IEnumerable<IScreensaverFactory> screensaverFactories)
        {
            ScreensaverFactories = screensaverFactories;
        }

        public bool ScreensaverIsRunning
        {
            get { return Application.Current.Windows.OfType<IScreensaver>().Any(); }
        }

        /// <summary>
        /// Stop screensaver running (if one is running)
        /// </summary>
        public void StopScreenSaver()
        {
            var screenSaver = Application.Current.Windows.OfType<IScreensaver>().FirstOrDefault();

            if (screenSaver != null)
            {
                screenSaver.Close();
            }
        }


        void _serverEvents_GeneralCommand(object sender, GeneralCommandEventArgs e)
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

            if (ScreensaverIsRunning)
            {
                StopScreenSaver();
            }
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            _lastInputTime = DateTime.Now;

            if (ScreensaverIsRunning)
            {
                StopScreenSaver();
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
            //_logger.Debug("TimerCallback {0} {1}", DateTime.Now, _lastInputTime);
            if ((DateTime.Now - _lastInputTime) >= TimeSpan.FromSeconds(ScreensaverIdleTimeoutSecs))
            {
                ShowScreensaver(false);
            }
        }
       
        /// <summary>
        /// Show  the current selected screen saver
        /// <param name="forceShowShowScreensaver">Show the Screensave even regardless of screensave timeout</param>
        /// </summary>
        public void ShowScreensaver(bool forceShowShowScreensaver)
        {
            var activeMedias = _playback.MediaPlayers
                .Where(i => i.PlayState == PlayState.Playing)
                .Select(i => i.CurrentMedia)
                .Where(i => i != null)
                .ToList();

            if (!forceShowShowScreensaver && activeMedias.Any(i => !i.IsAudio))
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
                var screenSaverFactory = ScreensaverFactories.FirstOrDefault(i => string.Equals(i.Name, _session.CurrentUser == null ? "Logo" : CurrentScreensaverName));
                if (screenSaverFactory != null)
                {
                    var screensaver = screenSaverFactory.GetScreensaver();

                    if (screensaver != null)
                    {
                        screensaver.ShowModal();
                    }
                }

                StartTimer();
            });
        }

        private void StartTimer()
        {
            _lastInputTime = DateTime.Now;

            if (_timer == null)
            {
                _timer = new Timer(TimerCallback, null, ScreensaverStartCheckMSec, ScreensaverStartCheckMSec); // check every x millisecond (default 30 seconds) if we shoud start the screen saver
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
            _serverEvents.GeneralCommand -= _serverEvents_GeneralCommand; 

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
