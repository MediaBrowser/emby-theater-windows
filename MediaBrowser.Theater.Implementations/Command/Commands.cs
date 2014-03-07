namespace MediaBrowser.Theater.Implementations.Command
{
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
}
