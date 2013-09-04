using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Core.Screensaver;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

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

        private DateTime _lastInputTime;
        private Timer _timer;

        public ScreensaverEntryPoint(IUserInputManager userInput, IPresentationManager presentationManager, IPlaybackManager playback, ISessionManager session, IApiClient apiClient, IImageManager imageManager)
        {
            _userInput = userInput;
            _presentationManager = presentationManager;
            _playback = playback;
            _session = session;
            _apiClient = apiClient;
            _imageManager = imageManager;
        }

        public void Run()
        {
            _playback.PlaybackCompleted += _playback_PlaybackCompleted;
            StartTimer();
        }

        void _playback_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            _lastInputTime = DateTime.Now;
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
            var activePlayer = _playback.MediaPlayers
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                // Never show when using an external player
                if (activePlayer is IExternalMediaPlayer)
                {
                    return;
                }

                var media = activePlayer.CurrentMedia;

                if (media != null)
                {
                    // For internal players, only allow the screen saver for audio
                    if (!media.IsAudio && activePlayer.PlayState != PlayState.Paused)
                    {
                        return;
                    }
                }
            }

            _presentationManager.Window.Dispatcher.InvokeAsync(ShowScreensaver);
        }

        private void ShowScreensaver()
        {
            StopTimer();

            new ScreensaverWindow(_session, _apiClient, _imageManager).ShowModal(_presentationManager.Window);

            StartTimer();
        }

        private void StartTimer()
        {
            _lastInputTime = DateTime.Now;

            if (_timer == null)
            {
                _timer = new Timer(TimerCallback, null, 1000, 1000);
            }

            PreventSystemIdle();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            AllowSystemIdle();
        }

        public void Dispose()
        {
            StopTimer();
        }

        private void PreventSystemIdle()
        {
            // Prevent system screen saver and monitor power off
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void AllowSystemIdle()
        {
            // Clear EXECUTION_STATE flags to disable away mode and allow the system to idle to sleep normally.
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
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
