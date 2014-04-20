namespace MediaBrowser.Theater.Interfaces.Commands
{
    public enum Command
    {
        // Global commands
        Null,               // does nothing, use for playholder
        Play,               // plays an item, arg is item, is none, plays the current item
        PlayPause,          // toggle play/pause the current item
        Pause,              // pause laying the current item
        UnPause,            // play the current item if it is paused
        TogglePause,        //
        Stop,               // stop playing the current item
        Queue,              // args, queue the args 
        FastForward,        // sequence through 1x, 2x, 4x, 8x, 16, 32x forward
        Rewind,             // sequence through  1x, 2x, 4x, 8x, 16, 32x in reverse
        PlaySpeedRatio,     // set play speed ratio , double, negative is rewind, positive is forward
        NextTrack,          // Next Media Track - chapter for video, track for audio
        PrevisousTrack,     // Previous Meda Track - - chapter for video, track for 
        Seek,
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
        SmallStepForward,  // 10 sec step forward
        SmallStepBack,     // 10 sec step forward
        StepForward,        // 30 sec step Back
        StepBack,          // 30 sec step Back
        BigStepForward,     // 5 Minutes setp fprward
        BigStepBack,       // 5 sec step Back
        FullScreen,        // set fullscreen, if it is full already, no change
        MinimizeScreen,    // Minimize screen
        RestoreScreen,     // restore screen to non full screen size, if not fullscreen, does nothing
        ToggleFullScreen,  // toggle fullscreen betweeen Fullscreen and windowed size
        SetVolume,         // arg 0.0 .. 100.0 %
        VolumeUp,          // volume up an inc
        VolumeDown,        // volumme an inc
        Mute,              // turn sound off
        UnMute,            // turn sound on
        ToggleMute,        // toggle sound on/off
        Alpha,             // arg is character - Alpha & puncs => build in command, does not need mapping
        Number,            // args is 0-9 => build in commond, does not need mapping
        SetSubtitleStreamIndex,         // Toggle subtitles
        NextSubtitleStream,             // sequence through subtitles
        SetAudioStreamIndex,
        NextAudioStream,
        AspectRatio,       // sequence through AspectRatios
        ShowOsd,           // Show On Screen Display
        HideOsd,           // Hides On Screen Display
        ToggleInfoPanel,   // Toggles info panel
        ShowInfoPanel,     // Show info panel
        HideinfoPanel,     // Hides info panel
        ToggleOsd,         // Toggles On Screen Display
        Close,             // close the app
        RestartMbt,        // restart this app
        RestartSystem,     // Restart the computer
        Logoff,            // Logoff the current User
        ShowScreensaver,   // display the screen saver
        // MoveItemUp etc,        // play list management
        ScreenDump
    }
}
