using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace MediaBrowser.UI.EntryPoints
{
    public class MediaCenterRemoteEntryPoint : IStartupEntryPoint
    {
        private readonly IPresentationManager _presenation;
        private readonly IPlaybackManager _playback;
        private readonly ILogger _logger;
        private readonly INavigationService _nav;
        private readonly IUserInputManager _userInput;

        public MediaCenterRemoteEntryPoint(IPresentationManager presenation, IPlaybackManager playback, ILogManager logManager, INavigationService nav, IUserInputManager userInput)
        {
            _presenation = presenation;
            _playback = playback;
            _nav = nav;
            _userInput = userInput;

            _logger = logManager.GetLogger(GetType().Name);
        }

        public void Run()
        {
            var window = _presenation.Window;

            window.Dispatcher.InvokeAsync(() =>
            {
                var source = (HwndSource)PresentationSource.FromVisual(window);

                source.AddHook(WndProc);
            });

            _nav.Navigated += _nav_Navigated;
        }

        void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NewPage is IFullscreenVideoPage)
            {
                RemoveWindowHook();
                AddGlobalHook();
            }
            else
            {
                RemoveGlobalHook();
                AddWindowHook();
            }
        }

        private void AddWindowHook()
        {
            _presenation.Window.KeyDown -= window_KeyDown;
            _presenation.Window.KeyDown += window_KeyDown;
        }
        private void RemoveWindowHook()
        {
            _presenation.Window.KeyDown -= window_KeyDown;
        }

        private void AddGlobalHook()
        {
            _userInput.KeyDown -= _userInput_KeyDown;
            _userInput.KeyDown += _userInput_KeyDown;
        }
        private void RemoveGlobalHook()
        {
            _userInput.KeyDown -= _userInput_KeyDown;
        }

        private const int WM_APPCOMMAND = 0x319;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var internallyHandled = false;

            // Handle messages...
            switch (msg)
            {
                case WM_APPCOMMAND:
                    internallyHandled = ProcessRemoteCommand(lParam.ToInt32() / 65536);
                    break;
            }

            if (internallyHandled)
            {
                handled = true;
            }

            return IntPtr.Zero;
        }

        public const int APPCOMMAND_BROWSER_BACKWARD = 1;
        public const int APPCOMMAND_BROWSER_FORWARD = 2;
        public const int APPCOMMAND_BROWSER_REFRESH = 3;
        public const int APPCOMMAND_BROWSER_STOP = 4;
        public const int APPCOMMAND_BROWSER_SEARCH = 5;
        public const int APPCOMMAND_BROWSER_FAVORITES = 6;
        public const int APPCOMMAND_BROWSER_HOME = 7;
        public const int APPCOMMAND_VOLUME_MUTE = 8;
        public const int APPCOMMAND_VOLUME_DOWN = 9;
        public const int APPCOMMAND_VOLUME_UP = 10;
        public const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
        public const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
        public const int APPCOMMAND_MEDIA_STOP = 13;
        public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        public const int APPCOMMAND_LAUNCH_MAIL = 15;
        public const int APPCOMMAND_LAUNCH_MEDIA_SELECT = 16;
        public const int APPCOMMAND_LAUNCH_APP1 = 17;
        public const int APPCOMMAND_LAUNCH_APP2 = 18;
        public const int APPCOMMAND_BASS_DOWN = 19;
        public const int APPCOMMAND_BASS_BOOST = 20;
        public const int APPCOMMAND_BASS_UP = 21;
        public const int APPCOMMAND_TREBLE_DOWN = 22;
        public const int APPCOMMAND_TREBLE_UP = 23;
        public const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24;
        public const int APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25;
        public const int APPCOMMAND_MICROPHONE_VOLUME_UP = 26;
        public const int APPCOMMAND_HELP = 27;
        public const int APPCOMMAND_FIND = 28;
        public const int APPCOMMAND_NEW = 29;
        public const int APPCOMMAND_OPEN = 30;
        public const int APPCOMMAND_CLOSE = 31;
        public const int APPCOMMAND_SAVE = 32;
        public const int APPCOMMAND_PRINT = 33;
        public const int APPCOMMAND_UNDO = 34;
        public const int APPCOMMAND_REDO = 35;
        public const int APPCOMMAND_COPY = 36;
        public const int APPCOMMAND_CUT = 37;
        public const int APPCOMMAND_PASTE = 38;
        public const int APPCOMMAND_REPLY_TO_MAIL = 39;
        public const int APPCOMMAND_FORWARD_MAIL = 40;
        public const int APPCOMMAND_SEND_MAIL = 41;
        public const int APPCOMMAND_SPELL_CHECK = 42;
        public const int APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43;
        public const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 44;
        public const int APPCOMMAND_CORRECTION_LIST = 45;
        public const int APPCOMMAND_MEDIA_PLAY = 46;
        public const int APPCOMMAND_MEDIA_PAUSE = 47;
        public const int APPCOMMAND_MEDIA_RECORD = 48; // Also 4144
        public const int APPCOMMAND_MEDIA_FAST_FORWARD = 49;
        public const int APPCOMMAND_MEDIA_REWIND = 50;
        public const int APPCOMMAND_MEDIA_CHANNEL_UP = 51;
        public const int APPCOMMAND_MEDIA_CHANNEL_DOWN = 52;

        /// <summary>
        /// Responds to multimedia keys
        /// </summary>
        private bool ProcessRemoteCommand(int cmd)
        {
            switch (cmd)
            {
                case APPCOMMAND_BROWSER_HOME:
                    ExecuteCommand(Home);
                    return true;
                case APPCOMMAND_MEDIA_PLAY_PAUSE:
                    ExecuteCommand(PlayPause);
                    return true;
                case APPCOMMAND_MEDIA_STOP:
                    ExecuteCommand(Stop);
                    return true;
                //case APPCOMMAND_MEDIA_NEXTTRACK:
                //    ExecuteCommand(OnNextTrackButton);
                //    return true;
                //case APPCOMMAND_MEDIA_PREVIOUSTRACK:
                //    ExecuteCommand(OnPreviousTrackButton);
                //    return true;
                case 4146:
                case APPCOMMAND_MEDIA_REWIND:
                    ExecuteCommand(SkipBackward);
                    return true;
                case 4145:
                case APPCOMMAND_MEDIA_FAST_FORWARD:
                    ExecuteCommand(SkipForward);
                    return true;
                case APPCOMMAND_CLOSE:
                    ExecuteCommand(Close);
                    return true;
                case 4142:
                case APPCOMMAND_MEDIA_PLAY:
                    ExecuteCommand(Play);
                    return true;
                case 4143:
                case APPCOMMAND_MEDIA_PAUSE:
                    ExecuteCommand(Pause);
                    return true;
                case APPCOMMAND_FIND:
                case APPCOMMAND_BROWSER_SEARCH:
                    ExecuteCommand(Search);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Responds to key presses inside a wpf window
        /// </summary>
        void window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.BrowserSearch:
                    ExecuteCommand(Search);
                    break;
                case System.Windows.Input.Key.BrowserHome:
                case System.Windows.Input.Key.Home:
                    ExecuteCommand(Home);
                    break;
                case System.Windows.Input.Key.Play:
                    ExecuteCommand(Play);
                    break;
                case System.Windows.Input.Key.Pause:
                    ExecuteCommand(Pause);
                    break;
                case System.Windows.Input.Key.MediaNextTrack:
                    ExecuteCommand(OnNextTrackButton);
                    break;
                case System.Windows.Input.Key.MediaPlayPause:
                    ExecuteCommand(PlayPause);
                    break;
                case System.Windows.Input.Key.MediaPreviousTrack:
                    ExecuteCommand(OnPreviousTrackButton);
                    break;
                case System.Windows.Input.Key.MediaStop:
                    ExecuteCommand(Stop);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Responds globally to key presses
        /// </summary>
        void _userInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.BrowserSearch:
                    ExecuteCommand(Search);
                    break;
                case Keys.BrowserHome:
                case Keys.Home:
                    ExecuteCommand(Home);
                    break;
                case Keys.Play:
                    ExecuteCommand(Play);
                    break;
                case Keys.Pause:
                    ExecuteCommand(Pause);
                    break;
                case Keys.MediaNextTrack:
                    ExecuteCommand(OnNextTrackButton);
                    break;
                case Keys.MediaPlayPause:
                    ExecuteCommand(PlayPause);
                    break;
                case Keys.MediaPreviousTrack:
                    ExecuteCommand(OnPreviousTrackButton);
                    break;
                case Keys.MediaStop:
                    ExecuteCommand(Stop);
                    break;
                default:
                    return;
            }
        }

        private readonly Task _trueTaskResult = Task.FromResult(true);

        private async void ExecuteCommand(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error executing command", ex);
            }
        }

        private async Task Play()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                if (activePlayer.PlayState == PlayState.Paused)
                {
                    await activePlayer.UnPause();
                    ShowFullscreenVideoOsd();
                }
            }
            else
            {
                await SendPlayCommandToPresentation();
            }
        }

        private async Task Pause()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                if (activePlayer.PlayState == PlayState.Paused)
                {
                    await activePlayer.UnPause();
                }
                else
                {
                    await activePlayer.Pause();
                }
                ShowFullscreenVideoOsd();
            }
        }

        private Task Close()
        {
            return _trueTaskResult;
        }

        private Task Search()
        {
            return _trueTaskResult;
        }

        private async Task SkipBackward()
        {
            var activePlayer = _playback.MediaPlayers
             .OfType<IInternalMediaPlayer>()
             .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                await activePlayer.SkipBackward();

                ShowFullscreenVideoOsd();
            }
        }

        private async Task SkipForward()
        {
            var activePlayer = _playback.MediaPlayers
             .OfType<IInternalMediaPlayer>()
             .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                await activePlayer.SkipForward();

                ShowFullscreenVideoOsd();
            }
        }

        private async Task PlayPause()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                if (activePlayer.PlayState == PlayState.Paused)
                {
                    await activePlayer.UnPause();
                }
                else
                {
                    await activePlayer.Pause();
                }

                ShowFullscreenVideoOsd();
            }
            else
            {
                await SendPlayCommandToPresentation();
            }
        }

        private Task SendPlayCommandToPresentation()
        {
            return _trueTaskResult;
        }

        private Task Stop()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                return activePlayer.Stop();
            }

            return _trueTaskResult;
        }

        private Task NextTrack()
        {
            return _trueTaskResult;
        }

        private Task PreviousTrack()
        {
            return _trueTaskResult;
        }

        private async Task NextChapter()
        {
            var activePlayer = _playback.MediaPlayers
              .OfType<IInternalMediaPlayer>()
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                await activePlayer.GoToNextChapter();

                ShowFullscreenVideoOsd();
            }
        }

        private async Task PreviousChapter()
        {
            var activePlayer = _playback.MediaPlayers
              .OfType<IInternalMediaPlayer>()
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                await activePlayer.GoToPreviousChapter();

                ShowFullscreenVideoOsd();
            }
        }

        private Task Home()
        {
            return _nav.NavigateToHomePage();
        }

        private Task YellowButton()
        {
            return _trueTaskResult;
        }

        private Task BlueButton()
        {
            return _trueTaskResult;
        }

        private Task RedButton()
        {
            return _trueTaskResult;
        }

        private Task GreenButton()
        {
            return _trueTaskResult;
        }

        private async Task OnNextTrackButton()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                await NextChapter();

                ShowFullscreenVideoOsd();
            }
            else
            {
                await NextTrack();
            }
        }

        private async Task OnPreviousTrackButton()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                await PreviousChapter();

                ShowFullscreenVideoOsd();
            }
            else
            {
                await PreviousTrack();
            }
        }

        private void ShowFullscreenVideoOsd()
        {
            var page = _nav.CurrentPage as IFullscreenVideoPage;

            if (page != null)
            {
                page.ShowOnScreenDisplay();
            }
        }
    }
}
