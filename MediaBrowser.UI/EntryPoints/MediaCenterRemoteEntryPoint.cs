using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.Osd;
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
using NLog;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

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

        private System.Windows.Input.Key _lastKeyDown;
        private DateTime _lastKeyDownTime;
        private int _lastCmd;
        private DateTime _lastCmdTime;
        private const double DuplicateCommandPeriod = 500;// Milliseconds


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
            _presenation.Window.KeyDown += window_KeyDown;
        }

        void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NewPage is IFullscreenVideoPage)
            {
                AddirectPlayWindowHook();
            }
            else
            {
                RemoveDirectPlayWindowHook();
           }
        }

        private void AddirectPlayWindowHook()
        {
            App.Instance.HiddenWindow.KeyDown -= directPlayWindow_KeyDown;
            App.Instance.HiddenWindow.KeyDown += directPlayWindow_KeyDown;
        }

        private void RemoveDirectPlayWindowHook()
        {
            App.Instance.HiddenWindow.KeyDown -= directPlayWindow_KeyDown;
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


// We need to handle 2 case of multiple key sequences for a single commmand
//
// its made a little more complex depending on wether the keydown event comes from a global hook or a window hook. The two
// hooks use different types for there Key code enums.
// 
// For now we just processing using a multiple methods and boolean constructs. We will move it to  more generic
// table drive logic when we refactor the key code mangtement to use a more sophisiphated key mapping architecture
//
//  1. some remotes produce a APPCOMMAND followed by a System.Windows.Forms.Keys Keycode or System.Windows.Input.Key Key (depending on hook method)
//        
//  Media Key types equivenlance table for media keys
//  APPCOMMAND code				            System.Windows.Forms.Keys	    System.Windows.Input.Key
//  APPCOMMAND_MEDIA_PLAY_PAUSE (14)    	MediaPlayPause (179)		    MediaPlayPause (135)
//  APPCOMMAND_MEDIA_STOP (13) 	    	    MediaStop (178)			        MediaStop = (134)
//  APPCOMMAND_MEDIA_NEXTTRACK (11)     	MediaNextTrack (176)		    MediaNextTrack = (132)
//  APPCOMMAND_MEDIA_PREVIOUSTRACK (12)     MediaPreviousTrack(177)		    MediaPreviousTrack = (133)
//
//  2. some remotes produce 2 Keycodes in sequence for 1 the one event, notably for left and right arrow
//  Right  &  LControlKey, LShiftKey, F   for fwd (>>)
//  Left  & LControlKey, LSHiftKey, B    for backward (<<) 
//
//  We dont currently handle Left & Right so we can ignore this condition for now
//
        private bool IsMediaCommand(int cmd)
        {
            return cmd == APPCOMMAND_MEDIA_NEXTTRACK ||
                   cmd == APPCOMMAND_MEDIA_PREVIOUSTRACK ||
                   cmd == APPCOMMAND_MEDIA_STOP ||
                   cmd == APPCOMMAND_MEDIA_PLAY_PAUSE;
        }

        private bool IsMediaCommand(System.Windows.Input.Key key)
        {
            return key == Key.MediaNextTrack ||
                   key == Key.MediaPreviousTrack ||
                   key == Key.MediaStop ||
                   key == Key.MediaPlayPause;
        }

        private bool MatchCommandWithWindowsKey(int cmd)
        {
            if ((cmd == APPCOMMAND_MEDIA_NEXTTRACK && _lastKeyDown == Key.MediaNextTrack) ||
                (cmd == APPCOMMAND_MEDIA_PREVIOUSTRACK && _lastKeyDown == Key.MediaPreviousTrack) ||
                (cmd == APPCOMMAND_MEDIA_STOP && _lastKeyDown == Key.MediaStop) ||
                (cmd == APPCOMMAND_MEDIA_PLAY_PAUSE && _lastKeyDown == Key.MediaPlayPause))
            {
                // its the same command, did it occur in the last DuplicateCommandPeriod Milliseconds
                TimeSpan span = DateTime.Now - _lastKeyDownTime;
                return span.TotalMilliseconds < DuplicateCommandPeriod;
            }
            else
            {
                return false;
            }
        }

        private bool MatchCommandWithWindowsKey(System.Windows.Input.Key key)
        {
            if ((_lastCmd == APPCOMMAND_MEDIA_NEXTTRACK && key == Key.MediaNextTrack) ||
                (_lastCmd == APPCOMMAND_MEDIA_PREVIOUSTRACK && key == Key.MediaPreviousTrack) ||
                (_lastCmd == APPCOMMAND_MEDIA_STOP && key == Key.MediaStop) ||
                (_lastCmd == APPCOMMAND_MEDIA_PLAY_PAUSE && key == Key.MediaPlayPause))
            {
                // its the same command, did it occur in the last DuplicateCommandPeriod Milliseconds
                TimeSpan span = DateTime.Now - _lastCmdTime;
                return span.TotalMilliseconds < DuplicateCommandPeriod;
            }
            else
            {
                return false;
            }
        }

        private bool IsDuplicateMediaKeyEvent(int cmd)
        {
            // A number of remote controls send a KeyDown event and thne a command event via WndProc for the following
            // media keys, PlayPause, Stop, Next, Previous, Fwd, Backwards. THis effect of this is to replicate the command
            // for these key. This double the effect of Next, Previous, Fwd, Backwards and starts & stops or stops and then starts
            // for PlayPause.
            // We therefore remember that type and time of the last media keyDown event and when we get a Media Key APPCOMMAND
            // get check if the correcponsing KetDown just occured and if it did ignore the APPCommand event

            return IsMediaCommand(cmd) && MatchCommandWithWindowsKey(cmd) ;
        }

        private bool IsDuplicateMediaKeyEvent(System.Windows.Input.Key key)
        {
            return IsMediaCommand(key) && MatchCommandWithWindowsKey(key);
        }

        /// <summary>
        /// Responds to multimedia keys
        /// ToDo
        ///  - Refactor ProcessRemoteCommand & ExecuteKeyCommand via
        ///    1. define a enum of commands, independant of Key or APPCOMMANDS
        ///    2. ExecuteCommand take a command and list of arguments (for latter key to command mapping)
        ///    3. Use a table to map APP_COMMANDS, Key with modifiers to mbt commands
        ///    4. External xml file generate table/dict what ever
        /// </summary>
        /// 
        private bool ProcessRemoteCommand(int cmd)
        {
            var handled = false;

            _logger.Debug("MediaCenterRemoteEntryPoint: ProcessRemoteCommand {0}", cmd);
            if (IsDuplicateMediaKeyEvent(cmd))
            {
                _logger.Debug("MediaCenterRemoteEntryPoint: IsDuplicate - cmd {0} after key {1}", cmd, _lastKeyDown);
                handled = true;
            }
            else
            {
                switch (cmd)
                {
                    case APPCOMMAND_INFO:
                        ExecuteCommand(Info);
                        handled = true;
                        break;

                    case APPCOMMAND_BROWSER_HOME:
                        ExecuteCommand(Home);
                        handled = true;
                        break;

                    case APPCOMMAND_MEDIA_PLAY_PAUSE:
                        ExecuteCommand(PlayPause);
                        handled = true;
                        break;

                    case APPCOMMAND_MEDIA_STOP:
                        ExecuteCommand(Stop);
                        handled = true;
                        break;

                    case APPCOMMAND_MEDIA_NEXTTRACK:
                        ExecuteCommand(OnNextTrackButton);
                        handled = true;
                        break;

                    case APPCOMMAND_MEDIA_PREVIOUSTRACK:
                        ExecuteCommand(OnPreviousTrackButton);
                        handled = true;
                        break;

                    case 4146:
                    case APPCOMMAND_MEDIA_REWIND:
                        ExecuteCommand(SkipBackward);
                        handled = true;
                        break;

                    case 4145:
                    case APPCOMMAND_MEDIA_FAST_FORWARD:
                        ExecuteCommand(SkipForward);
                        handled = true;
                        break;

                    case APPCOMMAND_CLOSE:
                        ExecuteCommand(Close);
                        handled = true;
                        break;

                    case 4142:
                    case APPCOMMAND_MEDIA_PLAY:
                        ExecuteCommand(Play);
                        handled = true;
                        break;

                    case 4143:
                    case APPCOMMAND_MEDIA_PAUSE:
                        ExecuteCommand(Pause);
                        handled = true;
                        break;

                    case APPCOMMAND_FIND:
                    case APPCOMMAND_BROWSER_SEARCH:
                        ExecuteCommand(Search);
                        handled = true;
                        break;

                    default:
                        handled = false;
                        break;
                } 
            }

            if (handled)
            {
                _lastCmd = cmd;
                _lastCmdTime = DateTime.Now;
            }
            else
            {
                _logger.Debug("MediaCenterRemoteEntryPoint: ProcessRemoteCommand {0}, command not implemented", cmd);
            }

            return handled;
        }

         private void ExecuteKeyCommand(System.Windows.Input.Key key, Boolean controlKeyDown,  Boolean shiftKeyDown)
        {
    
            _logger.Debug("MediaCenterRemoteEntryPoint: ExecuteKeyCommand {0} {1} {2}", key, shiftKeyDown, controlKeyDown);
            if (IsDuplicateMediaKeyEvent(key))
            {
                _logger.Debug("MediaCenterRemoteEntryPoint: IsDuplicate- Key {0} after cmd {1}", key, _lastCmd);
                return;
            }

            switch (key)
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
                        if (controlKeyDown)
                        {
                            if (shiftKeyDown)
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
                        if (controlKeyDown)
                        {
                            if (shiftKeyDown)
                            {
                                ExecuteCommand(Stop);
                            }
                            else
                            {
                                ExecuteCommand(Search);
                            }
                        }
                        break;
                    }
                case System.Windows.Input.Key.B:
                    {
                        if (controlKeyDown)
                        {
                            if (shiftKeyDown)
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
                        if (controlKeyDown)
                        {
                            if (shiftKeyDown)
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

                case System.Windows.Input.Key.D:
                    {
                        if (controlKeyDown)
                        {
                            ExecuteCommand(ToggleInfoPanel);
                        }
                        break;
                    }
                   

                case System.Windows.Input.Key.I:
                case System.Windows.Input.Key.Multiply:
                case System.Windows.Input.Key.D8:
                    {
                        ExecuteCommand(ToggleInfoPanel);
                        break;
                    }
                default:
                    break;
            }

            // record the key and the time but after the execute
             _lastKeyDown = key;
            _lastKeyDownTime = DateTime.Now;
        }

        /// <summary>
        /// Responds to key presses inside a wpf window
        /// </summary>
        void window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
           _logger.Debug("MediaCenterRemoteEntryPoint: window_KeyDown {0}", e.Key);
           ExecuteKeyCommand(e.Key, IsControlKeyDown(e), IsShiftKeyDown(e));
        }

        /// <summary>
        /// Responds to key presses in hidden window play via directpay
        /// </summary>
        //
        void directPlayWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
           _logger.Debug("MediaCenterRemoteEntryPoint:  {0}", e.KeyCode);
           ExecuteKeyCommand(KeyInterop.KeyFromVirtualKey((int)e.KeyCode), IsControlKeyDown(e), IsShiftKeyDown(e));
        }

        // WPF key mangement
        private bool IsShiftKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            return Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
        }

        private bool IsControlKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            return Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) ||
                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
        }

        // windows forms key management (events originate in hiddenform used in direct show player)
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
            _nav.NavigateToSearchPage();
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

    public static class Extensions
    {
        public static T SetFlag<T>(this Enum value, T flag, bool set)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());

            // note: AsInt mean: math integer vs enum (not the c# int type)
            dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
            dynamic flagAsInt = Convert.ChangeType(flag, underlyingType);
            if (set)
            {
                valueAsInt |= flagAsInt;
            }
            else
            {
                valueAsInt &= ~flagAsInt;
            }

            return (T)valueAsInt;
        }
    }

}

