using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Command;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Playback; // TODO - we should not need this - commands shold be implement in presentation
using System;
using System.Linq;
using System.Windows;
using WindowsInput = System.Windows.Input;
using System.Windows.Interop;

namespace MediaBrowser.Theater.Implementations.Command
{
    // received via WndProc
    public enum AppCommand
    {
        APPCOMMAND_BROWSER_BACKWARD = 1,
        APPCOMMAND_BROWSER_FORWARD = 2,
        APPCOMMAND_BROWSER_REFRESH = 3,
        APPCOMMAND_BROWSER_STOP = 4,
        APPCOMMAND_BROWSER_SEARCH = 5,
        APPCOMMAND_BROWSER_FAVORITES = 6,
        APPCOMMAND_BROWSER_HOME = 7,
        APPCOMMAND_VOLUME_MUTE = 8,
        APPCOMMAND_VOLUME_DOWN = 9,
        APPCOMMAND_VOLUME_UP = 10,
        APPCOMMAND_MEDIA_NEXTTRACK = 11,
        APPCOMMAND_MEDIA_PREVIOUSTRACK = 12,
        APPCOMMAND_MEDIA_STOP = 13,
        APPCOMMAND_MEDIA_PLAY_PAUSE = 14,
        APPCOMMAND_LAUNCH_MAIL = 15,
        APPCOMMAND_LAUNCH_MEDIA_SELECT = 16,
        APPCOMMAND_LAUNCH_APP1 = 17,
        APPCOMMAND_LAUNCH_APP2 = 18,
        APPCOMMAND_BASS_DOWN = 19,
        APPCOMMAND_BASS_BOOST = 20,
        APPCOMMAND_BASS_UP = 21,
        APPCOMMAND_TREBLE_DOWN = 22,
        APPCOMMAND_TREBLE_UP = 23,
        APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24,
        APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25,
        APPCOMMAND_MICROPHONE_VOLUME_UP = 26,
        APPCOMMAND_HELP = 27,
        APPCOMMAND_FIND = 28,
        APPCOMMAND_NEW = 29,
        APPCOMMAND_OPEN = 30,
        APPCOMMAND_CLOSE = 31,
        APPCOMMAND_SAVE = 32,
        APPCOMMAND_PRINT = 33,
        APPCOMMAND_UNDO = 34,
        APPCOMMAND_REDO = 35,
        APPCOMMAND_COPY = 36,
        APPCOMMAND_CUT = 37,
        APPCOMMAND_PASTE = 38,
        APPCOMMAND_REPLY_TO_MAIL = 39,
        APPCOMMAND_FORWARD_MAIL = 40,
        APPCOMMAND_SEND_MAIL = 41,
        APPCOMMAND_SPELL_CHECK = 42,
        APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43,
        APPCOMMAND_MIC_ON_OFF_TOGGLE = 44,
        APPCOMMAND_CORRECTION_LIST = 45,
        APPCOMMAND_MEDIA_PLAY = 46,
        APPCOMMAND_MEDIA_PLAY_2 = 4142,
        APPCOMMAND_MEDIA_PAUSE = 47,
        APPCOMMAND_MEDIA_PAUSE_2 = 4143,
        APPCOMMAND_MEDIA_RECORD = 48, 
        APPCOMMAND_MEDIA_RECORD_2 = 4144,
        APPCOMMAND_MEDIA_FAST_FORWARD = 49,
        APPCOMMAND_MEDIA_FAST_FORWARD_2 = 49,
        APPCOMMAND_MEDIA_REWIND = 50,
        APPCOMMAND_MEDIA_REWIND_2 = 4146,
        APPCOMMAND_MEDIA_CHANNEL_UP = 51,
        APPCOMMAND_MEDIA_CHANNEL_DOWN = 52,
        APPCOMMAND_CUSTOM = 200,
        // Future Commands:
        APPCOMMAND_OPENRECORDED = (APPCOMMAND_CUSTOM + 1),
        APPCOMMAND_LIVETV = (APPCOMMAND_CUSTOM + 2),
        APPCOMMAND_MENU = (APPCOMMAND_CUSTOM + 3),
        APPCOMMAND_GUIDEMENU = (APPCOMMAND_CUSTOM + 4),
        APPCOMMAND_CHANNELS = (APPCOMMAND_CUSTOM + 5),
        APPCOMMAND_INFO = (APPCOMMAND_CUSTOM + 6),
        APPCOMMAND_PROCAMP = (APPCOMMAND_CUSTOM + 7),
        APPCOMMAND_TIMESHIFT = (APPCOMMAND_CUSTOM + 8),
        APPCOMMAND_CC = (APPCOMMAND_CUSTOM + 9),
        APPCOMMAND_EPG = (APPCOMMAND_CUSTOM + 10),
        APPCOMMAND_CHANNEL_LAST = (APPCOMMAND_CUSTOM + 11),
        APPCOMMAND_ASP_STRETCH = (APPCOMMAND_CUSTOM + 20),
        APPCOMMAND_ASP_4X3 = (APPCOMMAND_CUSTOM + 21),
        APPCOMMAND_ASP_16X9 = (APPCOMMAND_CUSTOM + 22),
        APPCOMMAND_ASP_AUTO = (APPCOMMAND_CUSTOM + 23),
        APPCOMMAND_ASP_TOGGLE = (APPCOMMAND_CUSTOM + 24)
    } 

   
   



 
   
    public class CommandManager : ICommandManager
    {
        private readonly ILogger _logger;
        private readonly IUserInputManager _userInputManager;
        private readonly GlobalCommandActionMap _globalCommandActionMap;
        private readonly InputCommandMaps _inputCommandMaps;

        private WindowsInput.Key _lastKeyDown;
        private DateTime _lastKeyDownTime;
        private AppCommand _lastCmd;
        private DateTime _lastCmdTime;
        private const double DuplicateCommandPeriod = 500;// Milliseconds


        public CommandManager(IPresentationManager presentationManager, IPlaybackManager playbackManager, INavigationService navigationService, IUserInputManager userInputManager, ILogManager logManager)
        {
            _userInputManager = userInputManager;
            _inputCommandMaps = new InputCommandMaps();
            _globalCommandActionMap = new GlobalCommandActionMap(presentationManager, playbackManager, navigationService, logManager);

            _userInputManager.KeyDown += input_KeyDown;
            _userInputManager.AppCommand += input_AppCommand;

            _logger = logManager.GetLogger(GetType().Name);
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
            Boolean handled = false;
            
            var appCommand = MapAppCommand(appCommandEventArgs.Cmd);
            _logger.Debug("input_AppCommand: {0} {1}", appCommandEventArgs.Cmd, appCommand == null ? "null" : appCommand.ToString());

            if (appCommand != null)
            {
                if (IsDuplicateMediaKeyEvent(appCommand.Value))
                {
                    _logger.Debug("input_AppCommand: IsDuplicate - cmd {0} after key {1}", appCommand, _lastKeyDown);
                    handled = true;
                }
                else
                {
                    var command = _inputCommandMaps.GetMappedCommand(appCommand.Value);
                    handled = _globalCommandActionMap.ExecuteCommand(command);
                }
         
                if (handled)
                {
                    _lastCmd = appCommand.Value;
                    _lastCmdTime = DateTime.Now;
                }
                else
                {
                    _logger.Debug("input_AppCommand {0}, command not handled", appCommand);
                }
            }
           
           appCommandEventArgs.Handled = handled;
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
                _globalCommandActionMap.ExecuteCommand(command);
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
    }
}



