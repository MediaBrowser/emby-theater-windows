using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInput;
using System;
using MediaBrowser.Theater.Api.UserInterface;
using WindowsInput = System.Windows.Input;

namespace MediaBrowser.Theater.Api.Commands
{
   
    public class CommandManager : ICommandManager
    {
        private readonly ILogger _logger;
        private readonly IPresenter _presentationManager;
        private readonly IUserInputManager _userInputManager;
        private readonly InputCommandMaps _inputCommandMaps;

        private WindowsInput.Key _lastKeyDown;
        private DateTime _lastKeyDownTime;
        private AppCommand _lastCmd;
        private DateTime _lastCmdTime;
        private const double DuplicateCommandPeriod = 500;// Milliseconds


        public CommandManager(IPresenter presentationManager, IPlaybackManager playbackManager, INavigator navigationService, IUserInputManager userInputManager, ILogManager logManager)
        {
            _presentationManager = presentationManager;
            _userInputManager = userInputManager;
            _inputCommandMaps = new InputCommandMaps();

            _userInputManager.KeyDown += input_KeyDown;
            _userInputManager.AppCommand += input_AppCommand;

            _logger = logManager.GetLogger(GetType().Name);
        }

        public event CommandEventHandler CommandReceived;

        protected virtual void OnCommandReceived(CommandEventArgs commandeventargs)
        {
            CommandEventHandler handler = CommandReceived;
            if (handler != null) {
                handler(this, commandeventargs);
            }
        }

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
        // A number of remote controls send a KeyDown event and then a command event via WndProc for the following
        // media keys, PlayPause, Stop, Next, Previous, Fwd, Backwards. THis effect of this is to replicate the command
        // for these key. This double the effect of Next, Previous, Fwd, Backwards and starts & stops or stops and then starts
        // for PlayPause.
        // We therefore remember that type and time of the last media keyDown event and when we get a Media Key APPCOMMAND
        // get check if the correcponsing KetDown just occured and if it did ignore the APPCommand event

        private bool IsMediaCommand(AppCommand cmd)
        {
            return cmd == AppCommand.APPCOMMAND_MEDIA_NEXTTRACK ||
                   cmd == AppCommand.APPCOMMAND_MEDIA_PREVIOUSTRACK ||
                   cmd == AppCommand.APPCOMMAND_MEDIA_STOP ||
                   cmd == AppCommand.APPCOMMAND_MEDIA_PLAY_PAUSE;
        }

        private bool IsMediaCommand(WindowsInput.Key key)
        {
            return key == Key.MediaNextTrack ||
                   key == Key.MediaPreviousTrack ||
                   key == Key.MediaStop ||
                   key == Key.MediaPlayPause;
        }

        private bool MatchCommandWithWindowsKey(AppCommand cmd)
        {
            if ((cmd == AppCommand.APPCOMMAND_MEDIA_NEXTTRACK && _lastKeyDown == Key.MediaNextTrack) ||
                (cmd == AppCommand.APPCOMMAND_MEDIA_PREVIOUSTRACK && _lastKeyDown == Key.MediaPreviousTrack) ||
                (cmd == AppCommand.APPCOMMAND_MEDIA_STOP && _lastKeyDown == Key.MediaStop) ||
                (cmd == AppCommand.APPCOMMAND_MEDIA_PLAY_PAUSE && _lastKeyDown == Key.MediaPlayPause))
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

        private bool MatchCommandWithWindowsKey(WindowsInput.Key key)
        {
            if ((_lastCmd == AppCommand.APPCOMMAND_MEDIA_NEXTTRACK && key == Key.MediaNextTrack) ||
                (_lastCmd == AppCommand.APPCOMMAND_MEDIA_PREVIOUSTRACK && key == Key.MediaPreviousTrack) ||
                (_lastCmd == AppCommand.APPCOMMAND_MEDIA_STOP && key == Key.MediaStop) ||
                (_lastCmd == AppCommand.APPCOMMAND_MEDIA_PLAY_PAUSE && key == Key.MediaPlayPause))
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

        private bool IsDuplicateMediaKeyEvent(AppCommand cmd)
        {
          
            return IsMediaCommand(cmd) && MatchCommandWithWindowsKey(cmd) ;
        }

        private bool IsDuplicateMediaKeyEvent(WindowsInput.Key key)
        {
            return IsMediaCommand(key) && MatchCommandWithWindowsKey(key);
        }

        private AppCommand? MapAppCommand(int cmd)
        {
            AppCommand? appCommand = null;

            // dont use exception handling to exclude most frequent appCommand, its very slow
            // use an excludion test first that will catch most of teh cases
            if (cmd >= (int) AppCommand.APPCOMMAND_BROWSER_BACKWARD  || cmd <= (int) AppCommand.APPCOMMAND_ASP_TOGGLE)
            {
                try
                {
                    appCommand = (AppCommand)cmd;
                }
                catch (Exception)
                {
                    // not our app command
                }
            }
            
            return appCommand;
        }

        public void input_AppCommand(object sender, AppCommandEventArgs appCommandEventArgs)
        {
            var appCommand = MapAppCommand(appCommandEventArgs.Cmd);
            _logger.Debug("input_AppCommand: {0} {1}", appCommandEventArgs.Cmd, appCommand == null ? "null" : appCommand.ToString());

            if (appCommand != null)
            {
                if (IsDuplicateMediaKeyEvent(appCommand.Value))
                {
                    _logger.Debug("input_AppCommand: IsDuplicate - cmd {0} after key {1}", appCommand, _lastKeyDown);
                    appCommandEventArgs.Handled = false;
                }
                else if (!appCommandEventArgs.Handled) 
                {
                    var command = _inputCommandMaps.GetMappedCommand(appCommand.Value);
                    appCommandEventArgs.Handled = SendCommandEvents(command, null);
                }

                if (appCommandEventArgs.Handled)
                {
                    _lastCmd = appCommand.Value;
                    _lastCmdTime = DateTime.Now;
                }
                else
                {
                    _logger.Debug("input_AppCommand {0}, command not handled", appCommand);
                }
            }
        }
       
        /// <summary>
        /// Responds to key down in application
        /// </summary>
        void input_KeyDown(object sender, WindowsInput.KeyEventArgs e)
        {
            _logger.Debug("input_KeyDown {0} Ctrl({1}) Shift({2})", e.Key, IsControlKeyDown(), IsShiftKeyDown());
            if (IsDuplicateMediaKeyEvent(e.Key))
            {
                _logger.Debug("KeyDown IsDuplicateMediaKeyEvent true:- Key {0} after cmd {1}", e.Key, _lastCmd);
            }
            else
            {
                var command = _inputCommandMaps.GetMappedCommand(e.Key, IsControlKeyDown(), IsShiftKeyDown());
                SendCommandEvents(command, null);
            }

           _lastKeyDown = e.Key;
           _lastKeyDownTime = DateTime.Now;
        }

        private bool IsShiftKeyDown()
        {
            return Keyboard.IsKeyDown(WindowsInput.Key.LeftShift) || Keyboard.IsKeyDown(WindowsInput.Key.RightShift);
        }

        private bool IsControlKeyDown()
        {
            return Keyboard.IsKeyDown(WindowsInput.Key.LeftCtrl) || Keyboard.IsKeyDown(WindowsInput.Key.RightCtrl);
        }

        private bool SendCommandEvents(Command command, object args)
        {
            var target = _presentationManager.GetFocusedElement();

            // send to GUI handlers
            if (target != null) {
                if (SendTunneledEvent(target, command, args)) {
                    return true;
                }

                if (SendBubbledEvent(target, command, args)) {
                    return true;
                }
            }

            // send to global handlers
            var eventArgs = new CommandEventArgs { Command = command, Args = args };
            OnCommandReceived(eventArgs);
            return eventArgs.Handled;
        }

        private bool SendTunneledEvent(FrameworkElement target, Command command, object args)
        {
            var routedEvent = new CommandRoutedEventArgs(InputCommands.PreviewCommandSentEvent, _presentationManager.ActiveWindow)
            {
                Command = command,
                Args = args
            };

            target.RaiseEvent(routedEvent);

            return routedEvent.Handled;
        }

        private bool SendBubbledEvent(FrameworkElement target, Command command, object args)
        {
            var routedEvent = new CommandRoutedEventArgs(InputCommands.CommandSentEvent, _presentationManager.ActiveWindow)
            {
                Command = command,
                Args = args
            };

            target.RaiseEvent(routedEvent);

            return routedEvent.Handled;
        }
    }
}



