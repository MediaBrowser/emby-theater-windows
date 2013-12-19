using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Linq;
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
        private readonly IHiddenWindow _hiddenWindow;

        public MediaCenterRemoteEntryPoint(IPresentationManager presenation, IPlaybackManager playback, ILogManager logManager, INavigationService nav, IUserInputManager userInput, IHiddenWindow hiddenWindow)
        {
            _presenation = presenation;
            _playback = playback;
            _nav = nav;
            _userInput = userInput;
            _hiddenWindow = hiddenWindow;

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

        public const int APPCOMMAND_CUSTOM = 200;
        public const int APPCOMMAND_INFO = APPCOMMAND_CUSTOM + 6;

        // Future Commands:
//APPCOMMAND_OPENRECORDED (APPCOMMAND_CUSTOM + 1)
//APPCOMMAND_LIVETV (APPCOMMAND_CUSTOM + 2)
//APPCOMMAND_MENU (APPCOMMAND_CUSTOM + 3)
//APPCOMMAND_GUIDEMENU (APPCOMMAND_CUSTOM + 4)
//APPCOMMAND_CHANNELS (APPCOMMAND_CUSTOM + 5)
//APPCOMMAND_INFO (APPCOMMAND_CUSTOM + 6)
//APPCOMMAND_PROCAMP (APPCOMMAND_CUSTOM + 7)
//APPCOMMAND_TIMESHIFT (APPCOMMAND_CUSTOM + 8)
//APPCOMMAND_CC (APPCOMMAND_CUSTOM + 9)
//APPCOMMAND_EPG (APPCOMMAND_CUSTOM + 10)
//APPCOMMAND_CHANNEL_LAST (APPCOMMAND_CUSTOM + 11)

//APPCOMMAND_ASP_STRETCH (APPCOMMAND_CUSTOM + 20)
//APPCOMMAND_ASP_4X3 (APPCOMMAND_CUSTOM + 21)
//APPCOMMAND_ASP_16X9 (APPCOMMAND_CUSTOM + 22)
//APPCOMMAND_ASP_AUTO (APPCOMMAND_CUSTOM + 23)
//APPCOMMAND_ASP_TOGGLE (APPCOMMAND_CUSTOM + 24)

        /// <summary>
        /// Responds to multimedia keys
        /// </summary>
        private bool ProcessRemoteCommand(int cmd)
        {
            switch (cmd)
            {
                case APPCOMMAND_INFO:
                    ExecuteCommand(Info);
                    return true;
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
                case System.Windows.Input.Key.Space:
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
                case System.Windows.Input.Key.P:
                    {
                        if (IsControlKeyDown(e))
                        {
                            if (IsShiftKeyDown(e))
                            {
                                ExecuteCommand(Play);
                            }
                            else
                            {
                                ExecuteCommand(Pause);
                            }
                        }
                        break;
                    }
                case System.Windows.Input.Key.S:
                    {
                        if (IsControlKeyDown(e) && IsShiftKeyDown(e))
                        {
                            ExecuteCommand(Stop);
                        }
                        break;
                    }
                case System.Windows.Input.Key.B:
                    {
                        if (IsControlKeyDown(e))
                        {
                            if (IsShiftKeyDown(e))
                            {
                                ExecuteCommand(SkipBackward);
                            }
                            else
                            {
                                ExecuteCommand(OnPreviousTrackButton);
                            }
                        }
                        break;
                    }
                case System.Windows.Input.Key.F:
                    {
                        if (IsControlKeyDown(e))
                        {
                            if (IsShiftKeyDown(e))
                            {
                                ExecuteCommand(SkipForward);
                            }
                            else
                            {
                                ExecuteCommand(OnNextTrackButton);
                            }
                        }
                        break;
                    }
                case System.Windows.Input.Key.Multiply:
                    {
                        ExecuteCommand(ToggleInfoPanel);
                        break;
                    }
                case System.Windows.Input.Key.D8:
                    {
                        ExecuteCommand(ToggleInfoPanel);
                        break;
                    }
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
                case Keys.Space:
                case Keys.Pause:
                    ExecuteCommand(Pause);
                    break;
                case Keys.MediaPlayPause:
                    ExecuteCommand(PlayPause);
                    break;
                case Keys.MediaNextTrack:
                    ExecuteCommand(OnNextTrackButton);
                    break;
                case Keys.MediaPreviousTrack:
                    ExecuteCommand(OnPreviousTrackButton);
                    break;
                case Keys.MediaStop:
                    ExecuteCommand(Stop);
                    break;
                case Keys.P:
                    {
                        if (IsControlKeyDown(e))
                        {
                            if (IsShiftKeyDown(e))
                            {
                                ExecuteCommand(Play);
                            }
                            else
                            {
                                ExecuteCommand(Pause);
                            }
                        }
                        break;
                    }
                case Keys.S:
                    {
                        if (IsControlKeyDown(e) && IsShiftKeyDown(e))
                        {
                            ExecuteCommand(Stop);
                        }
                        break;
                    }
                case Keys.B:
                    {
                        if (IsControlKeyDown(e))
                        {
                            if (IsShiftKeyDown(e))
                            {
                                ExecuteCommand(SkipBackward);
                            }
                            else
                            {
                                ExecuteCommand(OnPreviousTrackButton);
                            }
                        }
                        break;
                    }
                case Keys.F:
                    {
                        if (IsControlKeyDown(e))
                        {
                            if (IsShiftKeyDown(e))
                            {
                                ExecuteCommand(SkipForward);
                            }
                            else
                            {
                                ExecuteCommand(OnNextTrackButton);
                            }
                        }
                        break;
                    }
                case Keys.D8:
                    {
                        ExecuteCommand(ToggleInfoPanel);
                        break;
                    }
                case Keys.Multiply:
                    ExecuteCommand(ToggleInfoPanel);
                    break;
                default:
                    return;
            }
        }

        private bool IsShiftKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            return e.Key.HasFlag(System.Windows.Input.Key.LeftShift) ||
                e.Key.HasFlag(System.Windows.Input.Key.RightShift) ||
                e.SystemKey.HasFlag(System.Windows.Input.Key.LeftShift) ||
                e.SystemKey.HasFlag(System.Windows.Input.Key.RightShift) ||
                e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
                e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightShift);
        }

        private bool IsControlKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            return e.Key.HasFlag(System.Windows.Input.Key.LeftCtrl) ||
                e.Key.HasFlag(System.Windows.Input.Key.RightCtrl) ||
                e.SystemKey.HasFlag(System.Windows.Input.Key.LeftCtrl) ||
                e.SystemKey.HasFlag(System.Windows.Input.Key.RightCtrl) ||
                e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl) ||
                e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightCtrl);
        }

        private bool IsShiftKeyDown(KeyEventArgs e)
        {
            return e.Shift || HasShift(e.Modifiers) || HasShift(Control.ModifierKeys);
        }

        private bool IsControlKeyDown(KeyEventArgs e)
        {
            return e.Control || HasControl(e.Modifiers) || HasControl(Control.ModifierKeys);
        }

        private bool HasControl(Keys keys)
        {
            return keys.HasFlag(Keys.Control) ||
                keys.HasFlag(Keys.ControlKey) ||
                keys.HasFlag(Keys.LControlKey) ||
                keys.HasFlag(Keys.RControlKey);
        }

        private bool HasShift(Keys keys)
        {
            return keys.HasFlag(Keys.Shift) ||
                keys.HasFlag(Keys.ShiftKey) ||
                keys.HasFlag(Keys.LShiftKey) ||
                keys.HasFlag(Keys.RShiftKey);
        }

        private void ExecuteCommand(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error executing command", ex);
            }
        }

        private void Play()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                if (activePlayer.PlayState == PlayState.Paused)
                {
                    activePlayer.UnPause();
                }

                ShowFullscreenVideoOsd();
            }
            else
            {
                SendPlayCommandToPresentation();
            }
        }

        private void Pause()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                if (activePlayer.PlayState == PlayState.Paused)
                {
                    activePlayer.UnPause();
                }
                else
                {
                    activePlayer.Pause();
                }
                ShowFullscreenVideoOsd();
            }
        }

        private void Close()
        {
        }

        private void Search()
        {
        }

        private void SkipBackward()
        {
            var activePlayer = _playback.MediaPlayers
             .OfType<IInternalMediaPlayer>()
             .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                activePlayer.SkipBackward();

                ShowFullscreenVideoOsd();
            }
        }

        private void SkipForward()
        {
            var activePlayer = _playback.MediaPlayers
             .OfType<IInternalMediaPlayer>()
             .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                activePlayer.SkipForward();

                ShowFullscreenVideoOsd();
            }
        }

        private void PlayPause()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                if (activePlayer.PlayState == PlayState.Paused)
                {
                    activePlayer.UnPause();
                }
                else
                {
                    activePlayer.Pause();
                }

                ShowFullscreenVideoOsd();
            }
            else
            {
                SendPlayCommandToPresentation();
            }
        }

        private void SendPlayCommandToPresentation()
        {
            _presenation.Window.Dispatcher.InvokeAsync(() =>
            {
                var currentPage = _nav.CurrentPage;

                var accepts = currentPage.DataContext as IAcceptsPlayCommand;

                if (accepts != null)
                {
                    accepts.HandlePlayCommand();
                }
            });
        }

        private void Stop()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                activePlayer.Stop();
            }
        }

        private void NextTrack()
        {
        }

        private void PreviousTrack()
        {
        }

        private void NextChapter()
        {
            var activePlayer = _playback.MediaPlayers
              .OfType<IInternalMediaPlayer>()
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                activePlayer.GoToNextChapter();

                ShowFullscreenVideoOsd();
            }
        }

        private void Info()
        {
            var activePlayer = _playback.MediaPlayers
              .OfType<IInternalMediaPlayer>()
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                ShowFullscreenVideoOsd();
            }
        }

        private void PreviousChapter()
        {
            var activePlayer = _playback.MediaPlayers
              .OfType<IInternalMediaPlayer>()
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null)
            {
                activePlayer.GoToPreviousChapter();

                ShowFullscreenVideoOsd();
            }
        }

        private void Home()
        {
            _nav.NavigateToHomePage();
        }

        private void YellowButton()
        {
        }

        private void BlueButton()
        {
        }

        private void RedButton()
        {
        }

        private void GreenButton()
        {
        }

        private void OnNextTrackButton()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                NextChapter();
            }
            else
            {
                NextTrack();
            }
        }

        private void OnPreviousTrackButton()
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                PreviousChapter();
            }
            else
            {
                PreviousTrack();
            }
        }

        private void ShowFullscreenVideoOsd()
        {
            _presenation.Window.Dispatcher.InvokeAsync(ShowFullscreenVideoOsdInternal);
        }

        private void ShowFullscreenVideoOsdInternal()
        {
            var page = _nav.CurrentPage as IFullscreenVideoPage;

            if (page != null)
            {
                page.ShowOnScreenDisplay();
            }
        }

        private void ToggleInfoPanel()
        {
            _presenation.Window.Dispatcher.InvokeAsync(ShowInfoPanelInternal);
        }

        private void ShowInfoPanelInternal()
        {
            var page = _nav.CurrentPage as IFullscreenVideoPage;

            if (page != null)
            {
                page.ToggleInfoPanel();
            }
        }
    }
}
