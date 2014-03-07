using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.Implementations.Command
{
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
            new InputCommandMapping( Key.D8,             Command.Info),
         
            new InputCommandMapping( Key.X,              Command.Stop),
            new InputCommandMapping( Key.OemPeriod,      Command.SkipNext),
            new InputCommandMapping( Key.OemComma,       Command.SkipPrevious),
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
            new InputCommandMapping( Key.MediaPreviousTrack,    Command.SkipPrevious),
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
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_REWIND,         Command.Rewind),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_REWIND_2,       Command.Rewind),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_FAST_FORWARD,   Command.FastForward),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_FAST_FORWARD_2, Command.FastForward),
            new InputCommandMapping( AppCommand.APPCOMMAND_CLOSE,                Command.Close),
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PLAY,           Command.Play),        
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PLAY_2,         Command.Play),  
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PAUSE,          Command.Pause),        
            new InputCommandMapping( AppCommand.APPCOMMAND_MEDIA_PAUSE_2,        Command.Pause),  
            new InputCommandMapping( AppCommand.APPCOMMAND_FIND,                 Command.GotoSearch),        
            new InputCommandMapping( AppCommand.APPCOMMAND_BROWSER_SEARCH,       Command.GotoSearch),

            // MBT specific & Test
            new InputCommandMapping( Key.S,  Command.GotoSearch,           controlKey:true, shiftKey:false),
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