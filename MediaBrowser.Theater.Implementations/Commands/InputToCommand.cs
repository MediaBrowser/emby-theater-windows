using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Interfaces.Commands;

namespace MediaBrowser.Theater.Implementations.Commands
{
    // received via WndProc
    internal enum AppCommand
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
        APPCOMMAND_MEDIA_FAST_FORWARD_2 = 4145,
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

    public class InputCommandMapGroup : Dictionary<string, InputCommandMap>
    {

    }

    public class InputCommandMap : List<InputCommandMapping>
    {
    }

    // Maps Iput - Key or AppComand to a command
    public class InputCommandMapping
    {
        internal Input  Input;
        internal Command Command;
        internal List<Object> Args;

        internal InputCommandMapping(Input input, Command command, List<Object> args = null)
        {
            this.Input = input;
            this.Command = command;
            Args = args;
        }

        internal InputCommandMapping(Key key, Command command, List<Object> args = null)
            : this(new Input(key), command, args)
        {
        }

        internal InputCommandMapping(Key key, Command command, Boolean controlKey, Boolean shiftKey, List<Object> args = null)
            : this(new Input(key, controlKey, shiftKey), command, args)
        {
        }

        internal InputCommandMapping(AppCommand appCommand, Command command)
            : this(new Input(appCommand), command)
        {
        }
    }


    internal enum InputType
    {
        AppCommand,
        Key,
        Remote
    };

    internal class Input
    {
        internal InputType InputType;
        internal Boolean ModShift;
        internal Boolean ModControl;
        internal Boolean ModAlt;
        internal Boolean ModFn;
        internal Boolean ModWindow;
        internal Key Key;
        internal AppCommand AppCommand;

        internal Input()
        {
        }

        internal Input(Key key)
        {
            this.Key = key;
            InputType = InputType.Key;
        }

        internal Input(Key key, Boolean controlKeyDown, Boolean shiftKeyDown)
        {
            this.Key = key;
            InputType = InputType.Key;
            ModControl = controlKeyDown;
            ModShift = shiftKeyDown;
        }

        internal Input(AppCommand appCommand)
        {
            this.AppCommand = appCommand;
            InputType = InputType.AppCommand;
        }

    }

    class InputCommandMaps
    {
        private  readonly InputCommandMap _defaultInputCommandMap = new InputCommandMap
        {
            new InputCommandMapping( Key.P,              Command.Play),
            new InputCommandMapping( Key.Play,           Command.Play),
            new InputCommandMapping( Key.F,              Command.FastForward),
            new InputCommandMapping( Key.Q,              Command.Queue),
            new InputCommandMapping( Key.R,              Command.Rewind),
            new InputCommandMapping( Key.MediaNextTrack,     Command.NextTrack),
            new InputCommandMapping( Key.MediaPreviousTrack, Command.PrevisousTrack),
            new InputCommandMapping( Key.Left,           Command.Left),
            new InputCommandMapping( Key.Right,          Command.Right),
            new InputCommandMapping( Key.Up,             Command.Up),
            new InputCommandMapping( Key.Down,           Command.Down),
            new InputCommandMapping( Key.PageUp,         Command.PageUp),
            new InputCommandMapping( Key.PageDown,       Command.PageDown),
            new InputCommandMapping( Key.Return,         Command.Select),
            new InputCommandMapping( Key.Enter,          Command.Select),
            new InputCommandMapping( Key.Back,           Command.Back),
            new InputCommandMapping( Key.I,              Command.Info),
            new InputCommandMapping( Key.D8,             Command.Info), // 8
            new InputCommandMapping( Key.D8,             Command.Info,  controlKey:false, shiftKey:true), // "*"
         
            new InputCommandMapping( Key.X,              Command.Stop),
            new InputCommandMapping( Key.OemPeriod,      Command.BigStepFoward),
            new InputCommandMapping( Key.OemComma,       Command.BigStepBack),
            //new CommandMap( Key.Tab ,           Command.ToggleFullScreen),
            new InputCommandMapping( Key.OemMinus,       Command.VolumeDown),
            new InputCommandMapping( Key.OemPlus,        Command.VolumeUp),
            new InputCommandMapping( Key.Subtract,       Command.VolumeDown),
            new InputCommandMapping( Key.Add,            Command.VolumeUp),
            new InputCommandMapping( Key.Oem5,           Command.ToggleFullScreen),
            new InputCommandMapping( Key.OemBackslash,   Command.ToggleFullScreen),
            new InputCommandMapping( Key.Home,           Command.FirstPage),
            new InputCommandMapping( Key.End,            Command.LastPage),
            // Multi Media Keys
            new InputCommandMapping( Key.BrowserBack,    Command.Back),
            new InputCommandMapping( Key.BrowserForward, Command.Forward),
            new InputCommandMapping( Key.BrowserRefresh, Command.Null),
            new InputCommandMapping( Key.BrowserStop,    Command.Stop),
            new InputCommandMapping( Key.BrowserSearch,  Command.GotoSearch),
            new InputCommandMapping( Key.BrowserHome,    Command.GotoHome),
            new InputCommandMapping( Key.VolumeMute,     Command.VolumeMute),
            new InputCommandMapping( Key.VolumeDown,     Command.VolumeDown),
            new InputCommandMapping( Key.VolumeUp,       Command.VolumeUp),
            new InputCommandMapping( Key.MediaNextTrack, Command.NextTrack),
            new InputCommandMapping( Key.MediaPreviousTrack,    Command.PrevisousTrack),
            new InputCommandMapping( Key.MediaStop,      Command.Stop),
            new InputCommandMapping( Key.MediaPlayPause, Command.PlayPause),
            // MS Media Center keyboard shortcuts sent by MCE remote -  http://msdn.microsoft.com/en-us/library/bb189249.aspx //
            new InputCommandMapping( Key.P,              Command.Play,           controlKey:true, shiftKey:true),
            new InputCommandMapping( Key.Space,          Command.PlayPause),
            new InputCommandMapping( Key.S,              Command.Stop,           controlKey:true, shiftKey:true),
            new InputCommandMapping( Key.P,              Command.Pause,          controlKey:true, shiftKey:false),
            new InputCommandMapping( Key.F,              Command.FastForward,    controlKey:true, shiftKey:true),
            new InputCommandMapping( Key.B,              Command.Rewind,         controlKey:true, shiftKey:true),
            new InputCommandMapping( Key.F,              Command.SkipNext,       controlKey:true, shiftKey:false),
            new InputCommandMapping( Key.B,              Command.SkipPrevious,   controlKey:true, shiftKey:false),
            new InputCommandMapping( Key.D,              Command.Info,           controlKey:true, shiftKey:false),
            new InputCommandMapping( Key.F10,            Command.VolumeUp),
            new InputCommandMapping( Key.F9,             Command.VolumeDown),
            new InputCommandMapping( Key.F8,             Command.VolumeMute),
            new InputCommandMapping( Key.G,              Command.OSD,           controlKey:true, shiftKey:false),

            // APP_COMMANDS
            new InputCommandMapping( AppCommand.APPCOMMAND_INFO,                 Command.Info),
            new InputCommandMapping( AppCommand.APPCOMMAND_BROWSER_HOME,         Command.GotoHome),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PLAY_PAUSE,     Command.PlayPause),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_STOP,           Command.Stop),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_NEXTTRACK,      Command.NextTrack),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PREVIOUSTRACK,  Command.PrevisousTrack),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_REWIND,         Command.SmallStepBack),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_REWIND_2,       Command.SmallStepBack),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_FAST_FORWARD,   Command.SmallStepForward),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_FAST_FORWARD_2, Command.SmallStepForward),
            new InputCommandMapping( AppCommand.APPCOMMAND_CLOSE,                Command.Close),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PLAY,           Command.PlayPause),        
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PLAY_2,         Command.PlayPause),  
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PAUSE,          Command.PlayPause),        
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PAUSE_2,        Command.PlayPause),  
            new InputCommandMapping( AppCommand.APPCOMMAND_FIND,                 Command.GotoSearch),        
            new InputCommandMapping( AppCommand.APPCOMMAND_BROWSER_SEARCH,       Command.GotoSearch),

            // MBT specific & Test
            new InputCommandMapping( Key.S,  Command.GotoSearch,           controlKey:true, shiftKey:false),
            new InputCommandMapping( Key.E,  Command.ShowScreensaver,      controlKey:true, shiftKey:true),
        };

        private readonly InputCommandMapGroup _inputCommandMapGroup;

        public InputCommandMaps()
        {
            _inputCommandMapGroup = new InputCommandMapGroup { { "global", _defaultInputCommandMap } };
        }

        public  Command GetMappedCommand(Key key, Boolean controlKeyDown, Boolean shiftKeyDown, String pageName = "")
        {

            var commandMap = (_inputCommandMapGroup.ContainsKey(pageName)) ? _inputCommandMapGroup[pageName] : _defaultInputCommandMap;
            var command = commandMap.FirstOrDefault(
                w =>
                    w.Input.InputType == InputType.Key && w.Input.Key == key && w.Input.ModShift == shiftKeyDown &&
                    w.Input.ModControl == controlKeyDown);

            return command != null ? command.Command : Command.Null;
        }

        public Command GetMappedCommand(AppCommand appCommand, String pageName = "")
        {
            var commandMap = (_inputCommandMapGroup.ContainsKey(pageName)) ? _inputCommandMapGroup[pageName] : _defaultInputCommandMap;
            var command = commandMap.FirstOrDefault(w => w.Input.InputType == InputType.AppCommand && w.Input.AppCommand == appCommand);

            return command != null ? command.Command : Command.Null;
        }

        public void LoadInputCommandMap(string xmlFilePath)
        {
            throw  new NotImplementedException();
        }

    }
}