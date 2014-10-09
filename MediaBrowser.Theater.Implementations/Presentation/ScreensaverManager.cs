using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Implementations.Presentation
{
    public class ScreensaverManager : IScreensaverManager, IDisposable
    {
        private const int ScreensaverIdleTimeoutSecs = 300; // timeout if we have been idle for 5 minutes - use the screen saver use 15 for debug
        private const int ScreensaverStartCheckMSec = 30000; // check idle timeout every 30 seconnds - use 1000 for debug

        private readonly IUserInputManager _userInput;
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playback;
        private readonly ISessionManager _session;
        private readonly ITheaterConfigurationManager _theaterConfigurationManager;
        private readonly ILogger _logger;

        private DateTime _lastInputTime;
        private Timer _timer;

        public ScreensaverManager(IUserInputManager userInput, IPresentationManager presentationManager, IPlaybackManager playback, ISessionManager session, ITheaterConfigurationManager theaterConfigurationManager, ILogManager logManager)
        {
            _userInput = userInput;
            _presentationManager = presentationManager;
            _playback = playback;
            _session = session;
            _theaterConfigurationManager = theaterConfigurationManager;
            _logger = logManager.GetLogger(GetType().Name);

            _session.UserLoggedIn += session_UserChanged;
            _session.UserLoggedOut += session_UserChanged;

            _playback.PlaybackCompleted += _playback_PlaybackCompleted;
            _playback.PlaybackStarted += _playback_PlaybackStarted;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            StartTimer();

            _session.UserLoggedIn += _session_UserLoggedIn;
        }

        void _session_UserLoggedIn(object sender, EventArgs e)
        {
            BindEvents(_session.ActiveApiClient);
        }

        private void BindEvents(IApiClient client)
        {
            UnbindEvents(client);

            client.BrowseCommand += _serverEvents_BrowseCommand;
            client.MessageCommand += _serverEvents_MessageCommand;
            client.PlayCommand += _serverEvents_PlayCommand;
            client.PlaystateCommand += _serverEvents_PlaystateCommand;
            client.GeneralCommand += client_GeneralCommand;
        }

        void client_GeneralCommand(object sender, GenericEventArgs<GeneralCommandEventArgs> e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        private void UnbindEvents(IApiClient client)
        {
            client.BrowseCommand -= _serverEvents_BrowseCommand;
            client.MessageCommand -= _serverEvents_MessageCommand;
            client.PlayCommand -= _serverEvents_PlayCommand;
            client.PlaystateCommand -= _serverEvents_PlaystateCommand;
            client.GeneralCommand -= client_GeneralCommand;
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
            get { return _presentationManager.Window.Dispatcher.Invoke(() => Application.Current.Windows.OfType<IScreensaver>().Any()); }
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

        void _serverEvents_PlaystateCommand(object sender, GenericEventArgs<PlaystateRequest> e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_PlayCommand(object sender, GenericEventArgs<PlayRequest> e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_MessageCommand(object sender, GenericEventArgs<MessageCommand> e)
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(OnRemoteControlCommand, DispatcherPriority.Background);
        }

        void _serverEvents_BrowseCommand(object sender, GenericEventArgs<BrowseRequest> e)
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
        public void ShowScreensaver(bool forceShowShowScreensaver, string overRideScreenSaverName = null)
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

            ShowScreensaver(overRideScreenSaverName);
        }

        private IScreensaverFactory GetSelectedScreensaverFactory(string overRideScreenSaverName)
        {
            var screensaverName = _session.CurrentUser == null ? "Logo" : overRideScreenSaverName ?? CurrentScreensaverName;

            return ScreensaverFactories.FirstOrDefault(i => string.Equals(i.Name, screensaverName));
        }

        private void ShowScreensaver(string overRideScreenSaverName)
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
                try
                {
                    var screensaverFactory = GetSelectedScreensaverFactory(overRideScreenSaverName);
                    if (screensaverFactory != null)
                    {
                        var screensaver = screensaverFactory.GetScreensaver();
                        Debug.Assert(screensaver != null);
                        _logger.Debug("Show screen saver {0}", screensaverFactory.Name);
                        screensaver.ShowModal();
                    }
                    else
                    {
                        _logger.Debug("Show screen saver - skip, none selected");
                    }
                }
                finally
                {
                    StartTimer();
                }

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
