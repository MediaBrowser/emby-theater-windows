using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Command;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MediaBrowser.Theater.Presentation.Playback;

namespace MediaBrowser.UI.Implementations
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

    public enum Command
    {
        // Global commands
        Null,               // does nothing, use for playholder
        Play,               // plays an item, arg is item, is none, plays the current item
        PlayPause,          // toggle play & pausethe current item
        Pause,              // pause laying the current item
        TogglePause,        //
        Stop,               // stop playing the current item
        Queue,              // args, queue the args 
        FastForward,        // sequence through 1x, 2x, 4x, 8x, 16, 32x forward
        Rewind,             // sequence through  1x, 2x, 4x, 8x, 16, 32x in reverse
        PlaySpeedRatio,     // set play speed ratio , double, negative is rewind, positive is foward
        NextTrack,          // Next Media Track - chapter for video, track for audio
        PrevisousTrack,     // Previous Meda Track - - chapter for video, track for 
        Left,
        Right,
        Up,
        Down,
        PageUp,
        PageDown,
        FirstPage,
        LastPage,
        Select,             // enter
        Back,               
        Forward,            // backpage
        GotoHome,
        GotoSearch,
        GotoSettings,       // top level settings page, can get a specific settings page via GotoPage
        GotoLogin,
        GotoPage,           // arg, page
        Info,
        SkipNext,          // Skip to next chapter in video. If no chapters, then skip to next item in playlist. 
        SkipPrevious,      // Skip to prev chapter in video. If no chapters, then skip to prev item in playlist. 
        Step,              // args - step in seconds, -ve  is step backward, +ve is forwards
        SmallStepForward,  // 10 sec step foward
        SmallStepBack,     // 10 sec step foward
        StepFoward,        // 30 sec step Back
        StepBack,          // 30 sec step Back
        BigStepFoward,     // 5 Minutes setp fprward
        BigStepBack,       // 5 sec step Back
        FullScreen,        // set fullscreen, if it is full already, no change
        MinimizeScreen,    // Minimize screen
        RestoreScreen,     // restore screen to non full screen size, if not fullscreen, does nothing
        ToggleFullScreen,  // toggle fullscreen betweeen Fullscreen and windowed size
        Volume,            // arg 0.0 .. 100.0 %
        VolumeUp,          // volume up an inc
        VolumeDown,        // volumme an inc
        VolumneOn,         // turn sound on
        VolumeOff,         // tunr sound off
        VolumeMute,        // toggle mute
        Alpha,             // arg is character - Alpha & puncs => build in command, does not need mapping
        Number,            // args is 0-9 => build in commond, does not need mapping
        Subtitles,         // Toggle subtitles
        NextSubtitle,      // sequence through subtitles
        AspectRatio,       // sequence through AspectRatios
        OSD,               // On Screen Display
        Close,             // close the app
        RestartMbt,        // restart this app
        RestartSystem,     // Restart the computer
        Logoff             // Logoff the current User
        // MoveItemUp etc,        // play list management
    }

    static class CommandMaps
    {
        public static CommandMapGroup CommandMap;

        public static CommandMapList GlobalCommandMap = new CommandMapList
        {
            new CommandMap( Key.P,              Command.Play),
            new CommandMap( Key.Play,           Command.Play),
            new CommandMap( Key.F,              Command.FastForward),
            new CommandMap( Key.Q,              Command.Queue),
            new CommandMap( Key.R,              Command.Rewind),
            new CommandMap( Key.MediaNextTrack,     Command.NextTrack),
            new CommandMap( Key.MediaPreviousTrack, Command.PrevisousTrack),
            new CommandMap( Key.Left,           Command.Left),
            new CommandMap( Key.Right,          Command.Right),
            new CommandMap( Key.Up,             Command.Up),
            new CommandMap( Key.Down,           Command.Down),
            new CommandMap( Key.PageUp,         Command.PageUp),
            new CommandMap( Key.PageDown,       Command.PageDown),
            new CommandMap( Key.Return,         Command.Select),
            new CommandMap( Key.Enter,          Command.Select),
            new CommandMap( Key.Back,           Command.Back),
            new CommandMap( Key.I,              Command.Info),
            new CommandMap( Key.D8,             Command.Info),
         
            new CommandMap( Key.X,              Command.Stop),
            new CommandMap( Key.OemPeriod,      Command.SkipNext),
            new CommandMap( Key.OemComma,       Command.SkipPrevious),
            //new CommandMap( Key.Tab ,           Command.ToggleFullScreen),
            new CommandMap( Key.OemMinus,       Command.VolumeDown),
            new CommandMap( Key.OemPlus,        Command.VolumeUp),
            new CommandMap( Key.Subtract,       Command.VolumeDown),
            new CommandMap( Key.Add,            Command.VolumeUp),
            new CommandMap( Key.Oem5,           Command.ToggleFullScreen),
            new CommandMap( Key.OemBackslash,   Command.ToggleFullScreen),
            new CommandMap( Key.Home,           Command.FirstPage),
            new CommandMap( Key.End,            Command.LastPage),
            // Multi Media Keys
            new CommandMap( Key.BrowserBack,    Command.Back),
            new CommandMap( Key.BrowserForward, Command.Forward),
            new CommandMap( Key.BrowserRefresh, Command.Null),
            new CommandMap( Key.BrowserStop,    Command.Stop),
            new CommandMap( Key.BrowserSearch,  Command.GotoSearch),
            new CommandMap( Key.BrowserHome,    Command.GotoHome),
            new CommandMap( Key.VolumeMute,     Command.VolumeMute),
            new CommandMap( Key.VolumeDown,     Command.VolumeDown),
            new CommandMap( Key.VolumeUp,       Command.VolumeUp),
            new CommandMap( Key.MediaNextTrack, Command.NextTrack),
            new CommandMap( Key.MediaPreviousTrack,    Command.SkipPrevious),
            new CommandMap( Key.MediaStop,      Command.Stop),
            new CommandMap( Key.MediaPlayPause, Command.PlayPause),
            // MS Media Center keyboard shortcuts sent by MCE remote -  http://msdn.microsoft.com/en-us/library/bb189249.aspx //
            new CommandMap( Key.P,              Command.Play,           controlKey:true, shiftKey:true),
            new CommandMap( Key.Space,          Command.PlayPause),
            new CommandMap( Key.S,              Command.Stop,           controlKey:true, shiftKey:true),
            new CommandMap( Key.P,              Command.Pause,          controlKey:true, shiftKey:false),
            new CommandMap( Key.F,              Command.FastForward,    controlKey:true, shiftKey:true),
            new CommandMap( Key.B,              Command.Rewind,         controlKey:true, shiftKey:true),
            new CommandMap( Key.F,              Command.SkipNext,       controlKey:true, shiftKey:false),
            new CommandMap( Key.B,              Command.SkipPrevious,   controlKey:true, shiftKey:false),
            new CommandMap( Key.D,              Command.Info,           controlKey:true, shiftKey:false),
            new CommandMap( Key.F10,            Command.VolumeUp),
            new CommandMap( Key.F9,             Command.VolumeDown),
            new CommandMap( Key.F8,             Command.VolumeMute),
            new CommandMap( Key.G,              Command.OSD,           controlKey:true, shiftKey:false),

            // APP_COMMANDS
            new CommandMap( AppCommand.APPCOMMAND_INFO,                 Command.Info),
            new CommandMap( AppCommand.APPCOMMAND_BROWSER_HOME,         Command.GotoHome),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_PLAY_PAUSE,     Command.PlayPause),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_STOP,           Command.Stop),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_NEXTTRACK,      Command.NextTrack),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_PREVIOUSTRACK,  Command.PrevisousTrack),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_REWIND,         Command.Rewind),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_REWIND_2,       Command.Rewind),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_FAST_FORWARD,   Command.FastForward),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_FAST_FORWARD_2, Command.FastForward),
            new CommandMap( AppCommand.APPCOMMAND_CLOSE,                Command.Close),
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_PLAY,           Command.Play),        
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_PLAY_2,         Command.Play),  
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_PAUSE,          Command.Pause),        
            new CommandMap( AppCommand.APPCOMMAND_MEDIA_PAUSE_2,        Command.Pause),  
            new CommandMap( AppCommand.APPCOMMAND_FIND,                 Command.GotoSearch),        
            new CommandMap( AppCommand.APPCOMMAND_BROWSER_SEARCH,       Command.GotoSearch),

            // MBT specific & Test
            new CommandMap( Key.S,  Command.GotoSearch,           controlKey:true, shiftKey:false),
        };

        static CommandMaps()
        {
            CommandMap = new CommandMapGroup();
            CommandMap.Add("Global", GlobalCommandMap);
        }

        public static Command MapInput(Key key, Boolean controlKeyDown, Boolean shiftKeyDown, String pageName = "")
        {

            var commandMap = (CommandMap.ContainsKey(pageName)) ? CommandMap[pageName] : GlobalCommandMap;
            var command = commandMap.FirstOrDefault(
                w =>
                    w.Input.InputType == InputType.Key && w.Input.Key == key && w.Input.ModShift == shiftKeyDown &&
                    w.Input.ModControl == controlKeyDown);

            return command != null ? command.Command : Command.Null;
        }

        public static Command MapInput(AppCommand appCommand, String pageName = "")
        {
            var commandMap = (CommandMap.ContainsKey(pageName)) ? CommandMap[pageName] : GlobalCommandMap;
            var command = commandMap.FirstOrDefault(w =>w.Input.InputType == InputType.AppCommand && w.Input.AppCommand == appCommand);

            return command != null ? command.Command : Command.Null;
        }
    }

    public class CommandMapGroup : Dictionary<string, CommandMapList>
    {

    }
    
    public class CommandMapList : List<CommandMap>
    {
    }

    public class CommandMap
    {
        public Input Input;
        public Command Command;
        public List<Object> Args;

        public CommandMap(Input input, Command command, List<Object> args = null)
        {
            this.Input = input;
            this.Command = command;
            Args = args;
        }

        public CommandMap(Key key, Command command,  List<Object> args = null) : this(new Input(key), command, args)
        {
        }

        public CommandMap(Key key, Command command,  Boolean controlKey, Boolean shiftKey, List<Object> args = null) : this(new Input(key, controlKey, shiftKey), command, args)
        {
        }

        public CommandMap(AppCommand appCommand, Command command) : this(new Input(appCommand), command)
        {
        }
    }

    public class CommandEventArgs
    {
        public Command Command;
        public Object Args;
        public Boolean Handled = false;
    }

    public delegate void ActionDel(Object sender, CommandEventArgs args);

    public class CommandAction
    {
        public Command Command;
        public ActionDel Action;
        public Object Args;

        public CommandAction(Command command, ActionDel action, Object args = null)
        {
            Command = command;
            Action = action;
            Args = args;
        }
    }

    public class CommandActionList : List<CommandAction>
    {
    }

    public enum InputType
    {
        AppCommand,
        Key,
        Remote
    };

    public class Input
    {
        public InputType InputType;
  
        public Boolean ModShift;
        public Boolean ModControl;
        public Boolean ModAlt;
        public Boolean ModFn;
        public Boolean ModWindow;
        public System.Windows.Input.Key Key;
        public AppCommand AppCommand;

        public Input()
        {
        }

        public Input(System.Windows.Input.Key key)
        {
            this.Key = key;
            InputType = InputType.Key;
        }

        public Input(System.Windows.Input.Key key, Boolean controlKeyDown, Boolean shiftKeyDown)
        {
            this.Key = key;
            InputType = InputType.Key;
            ModControl = controlKeyDown;
            ModShift = shiftKeyDown;
        }

        public Input(AppCommand appCommand)
        {
            this.AppCommand = appCommand;
            InputType = InputType.AppCommand;
        }

    }

    public class DefaultCommandActions
    {
        private readonly IPresentationManager _presenation;
        private readonly IPlaybackManager _playback;
        private readonly INavigationService _navigation;
        private readonly ILogger _logger;
        private readonly CommandAction _nullCommandAction;
        public CommandActionList _defaultdActionList;

        public DefaultCommandActions(IPresentationManager presenation, IPlaybackManager playback, INavigationService navigation, ILogManager logManager)
        {
            _presenation = presenation;
            _playback = playback;
            _navigation = navigation;
            _logger = logManager.GetLogger(GetType().Name);
            _defaultdActionList = DefaultGlobalCommandActionList();
            _nullCommandAction = new CommandAction(Command.Null, NullAction);
        }

        private CommandActionList DefaultGlobalCommandActionList()
        {
            var commandActionList = new CommandActionList
            {
                new CommandAction( Command.Null,            NullAction),
                new CommandAction( Command.Play,            Play),
                new CommandAction( Command.PlayPause,       PlayPause),
                new CommandAction( Command.Pause,           Pause),
                new CommandAction( Command.TogglePause,     TogglePause),
                new CommandAction( Command.Queue,           NullAction),
                new CommandAction( Command.FastForward,     NullAction),
                new CommandAction( Command.Rewind,          NullAction),
                new CommandAction( Command.PlaySpeedRatio,  NullAction),
                new CommandAction( Command.NextTrack,       NextTrackOrChapter),
                new CommandAction( Command.PrevisousTrack,  PreviousTrackOrChapter),
                new CommandAction( Command.Left,            NullAction),
                new CommandAction( Command.Right,           NullAction),
                new CommandAction( Command.Up,              NullAction),
                new CommandAction( Command.PageUp,          NullAction),
                new CommandAction( Command.PageDown,        NullAction),
                new CommandAction( Command.FirstPage,       NullAction),
                new CommandAction( Command.Rewind,          NullAction),
                new CommandAction( Command.PlaySpeedRatio,  NullAction),
                new CommandAction( Command.LastPage,        NullAction),
                new CommandAction( Command.Select,          NullAction),
                new CommandAction( Command.Back,            NullAction),
                new CommandAction( Command.Forward,         NullAction),
                new CommandAction( Command.GotoHome,        GotoHome),
                new CommandAction( Command.GotoSearch,      GotoSearch),
                new CommandAction( Command.GotoSettings,    GotoSettings),
                new CommandAction( Command.GotoPage,        Stop),
                new CommandAction( Command.Info,            Info),
                new CommandAction( Command.SkipNext,        SkipForward,        60),    // skip forward 60  seconds, boxed arguments
                new CommandAction( Command.SkipPrevious,    SkipBackward,       60),
                new CommandAction( Command.Step,            SkipForward,        60),     
                new CommandAction( Command.SmallStepForward,SkipForward,        10),
                new CommandAction( Command.SmallStepBack,   SkipBackward,       10),
                new CommandAction( Command.StepBack,        SkipBackward,       60),
                new CommandAction( Command.BigStepFoward,   SkipForward,        300),
                new CommandAction( Command.BigStepBack,     SkipBackward,       300),
                new CommandAction( Command.FullScreen,      FullScreen),
                new CommandAction( Command.MinimizeScreen,  MinimizeScreen),
                new CommandAction( Command.RestoreScreen,   RestoreScreen),
                new CommandAction( Command.ToggleFullScreen,ToggleFullscreen),
                new CommandAction( Command.Volume,          NullAction),
                new CommandAction( Command.VolumeUp,        NullAction),
                new CommandAction( Command.VolumeDown,      NullAction),
                new CommandAction( Command.VolumneOn,       NullAction),
                new CommandAction( Command.VolumeOff,       NullAction),
                new CommandAction( Command.VolumeMute,      NullAction),
                new CommandAction( Command.Subtitles,       NullAction),
                new CommandAction( Command.NextSubtitle,    NullAction),
                new CommandAction( Command.AspectRatio,     NullAction),
                new CommandAction( Command.OSD,             OSD)
            };

            return commandActionList;
        }

        private CommandAction MapCommand(Command command)
        {
            return _defaultdActionList.FirstOrDefault(a => a.Command == command) ?? _nullCommandAction;
        }

      
        private Boolean ExecuteCommandAction(CommandAction commandAction)
        {
            _logger.Debug("ExecuteCommandAction {0} {1}", commandAction.Command, commandAction.Args);
            var handled = false;

            try
            {
                var commandEventArgs = new CommandEventArgs()
                {
                    Args = commandAction.Args,
                    Command = commandAction.Command,
                    Handled = false
                };

                commandAction.Action(this, commandEventArgs);
                handled =  commandEventArgs.Handled;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error executing CommandAction", ex);
            }
            return handled;
        }

        public Boolean ExecuteCommand(Command command)
        {
            var commandAction = MapCommand(command);
            return ExecuteCommandAction(commandAction);
        }


        private void NullAction(Object sender, CommandEventArgs args)
        {
            // do nothing
        }

        private IInternalMediaPlayer GetActiveInternalMediaPlayer()
        {
            return _playback.MediaPlayers.OfType<IInternalMediaPlayer>().FirstOrDefault(i => i.PlayState != PlayState.Idle);
        }

        private void Play(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

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
            args.Handled = true;
        }

        private void PlayPause(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

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
            args.Handled = true;
        }

        private void Pause(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.PlayState != PlayState.Paused)
            {
                activePlayer.Pause();
                ShowFullscreenVideoOsd();
            }
            args.Handled = true;
        }

        private void TogglePause(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

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
            args.Handled = true;
        }

        private void Close()
        {
            // ToDo - close application gracefully
        }

        private void SkipBackward(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();
            if (activePlayer != null)
            {
                if (args.Args != null)
                {
                    activePlayer.SkipBackward((int)args.Args);
                }
                else
                {
                    activePlayer.SkipBackward();
                }
                ShowFullscreenVideoOsd();
            }
            args.Handled = true;
        }

        private void SkipForward(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                if (args.Args != null)
                {
                    activePlayer.SkipForward((int)args.Args);
                }
                else
                {
                    activePlayer.SkipForward();
                }
                ShowFullscreenVideoOsd();
            }
            args.Handled = true;
        }
       
        private void SendPlayCommandToPresentation()
        {
            _presenation.Window.Dispatcher.InvokeAsync(() =>
            {
                var currentPage = _navigation.CurrentPage;

                var accepts = currentPage.DataContext as IAcceptsPlayCommand;

                if (accepts != null)
                {
                    accepts.HandlePlayCommand();
                }
            });
        }

        private void Stop(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.Stop();
            }
            args.Handled = true;
        }

        private void NextTrack(Object sender, CommandEventArgs args)
        {
            // TODO - NextTrack Audio
        }

        private void PreviousTrack(Object sender, CommandEventArgs args)
        {
            // TODO - NextTrack Audio
        }

        private void NextChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.GoToNextChapter();
                ShowFullscreenVideoOsd();
            }
            args.Handled = true;
        }

        private void PreviousChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.GoToPreviousChapter();
                ShowFullscreenVideoOsd();
            }
            args.Handled = true;
        }

        private void NextTrackOrChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                NextChapter(sender, args);
            }
            else
            {
                NextTrack(sender, args);
            }
            args.Handled = true;
        }

        private void PreviousTrackOrChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = _playback.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                PreviousChapter(sender, args);
            }
            else
            {
                PreviousTrack(sender, args);
            }
            args.Handled = true;
        }

        private void OSD(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                ShowFullscreenVideoOsd();
            }
            args.Handled = true;
        }

        private void ShowFullscreenVideoOsd()
        {
            _presenation.Window.Dispatcher.InvokeAsync(ShowFullscreenVideoOsdInternal);
        }

        private void ShowFullscreenVideoOsdInternal()
        {
            var page = _navigation.CurrentPage as IFullscreenVideoPage;

            if (page != null)
            {
                page.ShowOnScreenDisplay();
            }
        }

        private void Info(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                ToggleInfoPanel();
            }
            args.Handled = true;

        }

        private void ToggleInfoPanel()
        {
            _presenation.Window.Dispatcher.InvokeAsync(ShowInfoPanelInternal);
        }

        private void ShowInfoPanelInternal()
        {
            var page = _navigation.CurrentPage as IFullscreenVideoPage;

            if (page != null)
            {
                page.ToggleInfoPanel();
            }
        }

        private void GotoHome(Object sender, CommandEventArgs args)
        {
            _navigation.NavigateToHomePage();
            args.Handled = true;
        }

        private void GotoSettings(Object sender, CommandEventArgs args)
        {
            _navigation.NavigateToSettingsPage();
            args.Handled = true;
        }

        private void GotoSearch(Object sender, CommandEventArgs args)
        {
            _navigation.NavigateToSearchPage();
            args.Handled = true;
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

        public class Interop
        {
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            public static IntPtr GetWindowHandle(Window window)
            {
                return new WindowInteropHelper(window).Handle;
            }
        }
      
        public void EnsureApplicationWindowHasFocus()
        {
            IntPtr window = Interop.GetWindowHandle(App.Instance.ApplicationWindow);
            IntPtr focused = Interop.GetForegroundWindow();
            if (window != focused)
            {
                Interop.SetForegroundWindow(window);
            }
        }

        public  void FullScreen(Object sender, CommandEventArgs args)
        {
            App.Instance.ApplicationWindow.WindowState = WindowState.Maximized;
            EnsureApplicationWindowHasFocus();
        }

        public void MinimizeScreen(Object sender, CommandEventArgs args)
        {
            App.Instance.ApplicationWindow.WindowState = WindowState.Minimized;
            EnsureApplicationWindowHasFocus();
        }

        public void RestoreScreen(Object sender, CommandEventArgs args)
        {
            App.Instance.ApplicationWindow.WindowState = WindowState.Normal;
            EnsureApplicationWindowHasFocus();
        }

        public  void ToggleFullscreen(Object sender, CommandEventArgs args)
        {
            if (App.Instance.ApplicationWindow.WindowState == WindowState.Maximized)
            {
                RestoreScreen(sender, args);
            }
            else
            {
                FullScreen(sender, args);
            }
        }
    }
   
    public class CommandManager : ICommandManager
    {
        private readonly ILogger _logger;
        private readonly IUserInputManager _userInputManager;
        private readonly DefaultCommandActions _defaultCommandActions;

        private System.Windows.Input.Key _lastKeyDown;
        private DateTime _lastKeyDownTime;
        private AppCommand _lastCmd;
        private DateTime _lastCmdTime;
        private const double DuplicateCommandPeriod = 500;// Milliseconds


        public CommandManager(IPresentationManager presentationManager, IPlaybackManager playbackManager, INavigationService navigationService, IUserInputManager userInputManager, ILogManager logManager)
        {
            _userInputManager = userInputManager;
            _defaultCommandActions = new DefaultCommandActions(presentationManager, playbackManager, navigationService, logManager);

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

        private bool IsMediaCommand(System.Windows.Input.Key key)
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

        private bool MatchCommandWithWindowsKey(System.Windows.Input.Key key)
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

        private bool IsDuplicateMediaKeyEvent(System.Windows.Input.Key key)
        {
            return IsMediaCommand(key) && MatchCommandWithWindowsKey(key);
        }

        private AppCommand? MapAppCommand(int cmd)
        {
            AppCommand? appCommand = null;

            // dont use exception handling to exclude most frequent appCommand, its very slow
            // use a aucik excludion test first that will catch most of teh cases
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
                    var command = CommandMaps.MapInput(appCommand.Value);
                     handled = _defaultCommandActions.ExecuteCommand(command);
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
        void input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _logger.Debug("input_KeyDown {0} Ctrl({1}) Shift({2})", e.Key, IsControlKeyDown(), IsShiftKeyDown());
            if (IsDuplicateMediaKeyEvent(e.Key))
            {
                _logger.Debug("KeyDown IsDuplicateMediaKeyEvent true:- Key {0} after cmd {1}", e.Key, _lastCmd);
            }
            else
            {
                var command = CommandMaps.MapInput(e.Key, IsControlKeyDown(), IsShiftKeyDown());
                _defaultCommandActions.ExecuteCommand(command);
            }

           _lastKeyDown = e.Key;
           _lastKeyDownTime = DateTime.Now;
        }

        private bool IsShiftKeyDown()
        {
            return Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
        }

        private bool IsControlKeyDown()
        {
            return Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) ||
                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
        }
    }
}



