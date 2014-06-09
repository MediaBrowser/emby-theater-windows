using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.EntryPoints.CommandActions
{

    internal delegate void ActionDelagate(Object sender, CommandEventArgs args);

    internal class CommandActionMapping
    {
        public Command Command;
        public ActionDelagate Action;
        public Object Args;

        public CommandActionMapping(Command command, ActionDelagate action, Object args = null)
        {
            Command = command;
            Action = action;
            Args = args;
        }
    }

    internal class CommandActionMap : List<CommandActionMapping>
    {
    }

    internal class DefaultCommandActionMap
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly IPresenter _presenation;
        private readonly IPlaybackManager _playback;
        private readonly INavigator _navigation;
//        private readonly IScreensaverManager _screensaverManager;
        private readonly ILogger _logger;
        private readonly CommandActionMapping _nullCommandActionMapping;
        private readonly CommandActionMap _globalCommandActionMap;
        private double _currentPlaybackRate = 1.0; // Todo - move to reportable property of IPlaybackManager

        public DefaultCommandActionMap(ITheaterApplicationHost appHost, IPresenter presenation, IPlaybackManager playback, INavigator navigation, /*IScreensaverManager screensaverManager,*/ ILogManager logManager, IEventAggregator events)
        {
            _appHost = appHost;
            _presenation = presenation;
            _playback = playback;
//            _screensaverManager = screensaverManager;
            _navigation = navigation;
            _logger = logManager.GetLogger(GetType().Name);
            _globalCommandActionMap = CreateGlobalCommandActionMap();
            _nullCommandActionMapping = new CommandActionMapping(Command.Null, NullAction);

            events.Get<PlaybackStartEventArgs>().Subscribe(arg => _currentPlaybackRate = 1.0);
            events.Get<PlaybackStopEventArgs>().Subscribe(arg => _currentPlaybackRate = 1.0);
        }

        private CommandActionMap CreateGlobalCommandActionMap()
        {
            return new CommandActionMap
            {
                new CommandActionMapping( Command.Null,            NullAction),
                new CommandActionMapping( Command.Play,            Play),
                new CommandActionMapping( Command.PlayPause,       PlayPause),
                new CommandActionMapping( Command.Pause,           Pause),
                new CommandActionMapping( Command.UnPause,         UnPause),
                new CommandActionMapping( Command.Stop,            Stop),
                new CommandActionMapping( Command.TogglePause,     TogglePause),
                new CommandActionMapping( Command.Queue,           NullAction),
                new CommandActionMapping( Command.FastForward,     FastForward),
                new CommandActionMapping( Command.Rewind,          Rewind),
                new CommandActionMapping( Command.PlaySpeedRatio,  NullAction),
                new CommandActionMapping( Command.NextChapter,     NextChapter),
                new CommandActionMapping( Command.PreviousChapter, PreviousChapter),
                new CommandActionMapping( Command.NextTrack,       NextTrack),
                new CommandActionMapping( Command.PreviousTrack,   PreviousTrack),
                new CommandActionMapping( Command.NextTrackOrChapter,       NextTrackOrChapter),
                new CommandActionMapping( Command.PreviousTrackOrChapter,   PreviousTrackOrChapter),
                new CommandActionMapping( Command.Seek,            Seek),
                new CommandActionMapping( Command.Left,            NullAction),
                new CommandActionMapping( Command.Right,           NullAction),
                new CommandActionMapping( Command.Up,              NullAction),
                new CommandActionMapping( Command.PageUp,          NullAction),
                new CommandActionMapping( Command.PageDown,        NullAction),
                new CommandActionMapping( Command.FirstPage,       NullAction),
                new CommandActionMapping( Command.Rewind,          NullAction),
                new CommandActionMapping( Command.PlaySpeedRatio,  NullAction),
                new CommandActionMapping( Command.LastPage,        NullAction),
                new CommandActionMapping( Command.Select,          NullAction),
                new CommandActionMapping( Command.Back,            NavigateBack),
                new CommandActionMapping( Command.Forward,         NavigateForward),
                new CommandActionMapping( Command.GotoHome,        GoToHome),
                new CommandActionMapping( Command.GotoSearch,      GoToSearch),
                new CommandActionMapping( Command.GotoSettings,    GoToSettings),
                new CommandActionMapping( Command.GotoPage,        NullAction),
                //new CommandActionMapping( Command.Info,            ToggleInfoPanel),
                new CommandActionMapping( Command.SkipNext,        SkipForward,        60),    // skip forward 60  seconds, boxed arguments
                new CommandActionMapping( Command.SkipPrevious,    SkipBackward,       60),
                new CommandActionMapping( Command.Step,            SkipForward,        60),     
                new CommandActionMapping( Command.SmallStepForward,SkipForward,        10),
                new CommandActionMapping( Command.SmallStepBack,   SkipBackward,       10),
                new CommandActionMapping( Command.StepForward,     SkipBackward,       60),
                new CommandActionMapping( Command.StepBack,        SkipBackward,       60),
                new CommandActionMapping( Command.BigStepForward,  SkipForward,        300),
                new CommandActionMapping( Command.BigStepBack,     SkipBackward,       300),
                new CommandActionMapping( Command.FullScreen,               FullScreen),
                new CommandActionMapping( Command.MinimizeScreen,           MinimizeScreen),
                new CommandActionMapping( Command.RestoreScreen,            RestoreScreen),
                new CommandActionMapping( Command.ToggleFullScreen,         ToggleFullscreen),
                new CommandActionMapping( Command.SetVolume,                SetVolume),
                new CommandActionMapping( Command.VolumeUp,                 (s,e) => _playback.VolumeStepUp()),
                new CommandActionMapping( Command.VolumeDown,               (s,e) => _playback.VolumeStepDown()),
                new CommandActionMapping( Command.Mute,                     (s,e) => _playback.Mute()),
                new CommandActionMapping( Command.UnMute,                   (s,e) => _playback.UnMute()),
                new CommandActionMapping( Command.ToggleMute,               (s,e) => _playback.ToggleMute()),
                new CommandActionMapping( Command.SetSubtitleStreamIndex,   SetSubtitleStreamIndex),
                new CommandActionMapping( Command.NextSubtitleStream,       (s, a) => _playback.NextSubtitleStream()),
                new CommandActionMapping( Command.SetAudioStreamIndex,      SetAudioStreamIndex),
                new CommandActionMapping( Command.NextAudioStream,          (s, a) => _playback.NextAudioStream()),
                new CommandActionMapping( Command.AspectRatio,              NullAction), // ToDo
//                new CommandActionMapping( Command.ShowOsd,                  ShowOsd),
//                new CommandActionMapping( Command.HideOsd,                  HideOSd),
//                new CommandActionMapping( Command.ToggleOsd,                ToggleOsd),
//                new CommandActionMapping( Command.ShowInfoPanel,            ShowInfoPanel),
//                new CommandActionMapping( Command.HideinfoPanel,            HideInfoPanel),
//                new CommandActionMapping( Command.ToggleInfoPanel,          ToggleInfoPanel),
//                new CommandActionMapping( Command.ShowScreensaver,          (s, a) => _screensaverManager.ShowScreensaver(true)),
//                new CommandActionMapping( Command.ScreenDump,               (s, a) => MBTScreenDump.GetAndSaveWindowsImage(_presenation.MainApplicationWindowHandle)),


            };
        }

        private CommandActionMapping MapCommand(Command command)
        {
            return _globalCommandActionMap.FirstOrDefault(a => a.Command == command) ?? _nullCommandActionMapping;
        }

        public Boolean ExecuteCommand(Command command, Object args)
        {
            var commandAction = MapCommand(command);
            return ExecuteCommandAction(commandAction, args);
        }

        private Boolean ExecuteCommandAction(CommandActionMapping commandActionMapping, Object args)
        {
            _logger.Debug("ExecuteCommandAction {0} {1}", commandActionMapping.Command, commandActionMapping.Args);
            var handled = false;

            try
            {
                var commandEventArgs = new CommandEventArgs()
                {
                    Args = args ?? commandActionMapping.Args,
                    Command = commandActionMapping.Command,
                    Handled = false
                };

                commandActionMapping.Action(this, commandEventArgs);
                handled = commandEventArgs.Handled;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error executing CommandAction", ex);
            }
            return handled;
        }

        private void NullAction(Object sender, CommandEventArgs args)
        {
            // do nothing
        }

        private void NavigateBack(Object sender, CommandEventArgs args)
        {
            _navigation.Back();
        }

        private void NavigateForward(Object sender, CommandEventArgs args)
        {
            _navigation.Forward();
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

                //ShowFullscreenVideoOsd();
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

                //ShowOsd(sender, args);
            }
           
            args.Handled = true;
        }

        private void Pause(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.PlayState != PlayState.Paused)
            {
                activePlayer.Pause();
                //ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void UnPause(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.PlayState == PlayState.Paused)
            {
                activePlayer.UnPause();
                //ShowOsd(sender, args);
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

                //ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void Close()
        {
            _appHost.Shutdown();
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
                //ShowOsd(sender, args);
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
                //ShowOsd(sender, args);
            }
            args.Handled = true;
        }

//        private void SendPlayCommandToPresentation()
//        {
//            _presenation.Window.Dispatcher.InvokeAsync(() =>
//            {
//                var currentPage = _navigation.CurrentPage;
//
//                var accepts = currentPage.DataContext as IAcceptsPlayCommand;
//
//                if (accepts != null)
//                {
//                    accepts.HandlePlayCommand();
//                }
//            });
//        }

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
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.NextTrack();
                //ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void PreviousTrack(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.PreviousTrack();
                //ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void NextChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.GoToNextChapter();
                //ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void PreviousChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.GoToPreviousChapter();
                //ShowOsd(sender, args);
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

        // todo - fastforwad doubles the forward speed, also need an inc that increments it by 1
        private void FastForward(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                _currentPlaybackRate = _currentPlaybackRate * 2.0;
                activePlayer.SetRate(_currentPlaybackRate);
            }
            args.Handled = true;
        }

        private void Rewind(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.CurrentMedia != null && activePlayer.CurrentMedia.IsVideo)
            {
                _currentPlaybackRate = _currentPlaybackRate/2.0;
               activePlayer.SetRate(_currentPlaybackRate);
            }

            args.Handled = true;
        }

        void Seek(Object sender, CommandEventArgs args)
        {
            _logger.Debug("Seek  {0}", args.Args);

            long position;

            if (args == null)
                throw new ArgumentException("sender: expecting a single long argument");

            try
            {
                position = (long)Convert.ToUInt64(args.Args);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Seek: Invalid format, expecting a single float 0..100 argurment for SetVolume");
            }


            var player = GetActiveInternalMediaPlayer();

            if (player != null)
            {
                player.Seek(position);
            }

        }

//        private void OSD(Object sender, CommandEventArgs args)
//        {
//            var activePlayer = GetActiveInternalMediaPlayer();
//
//            if (activePlayer != null)
//            {
//                ShowFullscreenVideoOsd();
//            }
//            args.Handled = true;
//        }

//        private void ShowFullscreenVideoOsd()
//        {
//            _presenation.Window.Dispatcher.InvokeAsync(ShowFullscreenVideoOsdInternal);
//        }
//
//        private void ShowFullscreenVideoOsdInternal()
//        {
//            var page = _navigation.CurrentPage as IFullscreenVideoPage;
//
//            if (page != null)
//            {
//                page.ShowOnScreenDisplay();
//            }
//        }

//        private void Info(Object sender, CommandEventArgs args)
//        {
//            var activePlayer = GetActiveInternalMediaPlayer();
//
//            if (activePlayer != null)
//            {
//                ToggleInfoPanel();
//            }
//            args.Handled = true;
//
//        }

//        private void ToggleInfoPanel()
//        {
//            _presenation.Window.Dispatcher.InvokeAsync(ShowInfoPanelInternal);
//        }
//
//        private void ShowInfoPanelInternal()
//        {
//            var page = _navigation.CurrentPage as IFullscreenVideoPage;
//
//            if (page != null)
//            {
//                page.ToggleInfoPanel();
//            }
//        }

        private void GoToHome(Object sender, CommandEventArgs args)
        {
            _navigation.Navigate(Go.To.Home());
            args.Handled = true;
        }

        private void GoToSettings(Object sender, CommandEventArgs args)
        {
            _navigation.Navigate(Go.To.Settings());
            args.Handled = true;
        }

        private void GoToSearch(Object sender, CommandEventArgs args)
        {
            _navigation.Navigate(Go.To.Search());
            args.Handled = true;
        }

        void SetVolume(Object sender, CommandEventArgs args)
        {
            _logger.Debug("SetVolume  {0}", args);

            float volume;

            if (args == null)
                throw new ArgumentException("SetVolume: expecting a single float 0..100 argurment for SetVolume");

            try
            {
                volume = (float)Convert.ToDouble(args);
            }
            catch (FormatException)
            {
                throw new ArgumentException("SetVolume: Invalid format, expecting a single float 0..100 argurment for SetVolume");
            }

            if (volume < 0.0 || volume > 100.0)
            {
                throw new ArgumentException(string.Format("SetVolume: Invalid Volume {0}. Volume range is 0..100", volume));
            }

            _playback.SetVolume(volume);
        }

        void SetAudioStreamIndex(Object sender, CommandEventArgs args)
        {
            _logger.Debug("SetAudioStreamIndex {0}", args);

            int index;

            if (args == null)
                throw new ArgumentException("SetAudioStreamIndex: expecting a single integer argurment for AudiostreamIndex");

            try
            {
                index = Convert.ToInt32(args);
            }
            catch (FormatException)
            {
                throw new ArgumentException("SetAudioStreamIndex: Invalid format, expecting a single integer argurment for AudiostreamIndex");
            }


            _playback.SetAudioStreamIndex(index);
        }

        void SetSubtitleStreamIndex(object sender, CommandEventArgs args)
        {
            _logger.Debug("SetSubtitleStreamIndex {0}", args);

            int index;

            if (args == null)
                throw new ArgumentException("SetSubtitleStreamIndex: expecting a single integer argurment for SubtitleStreamIndex");

            try
            {
                index = Convert.ToInt32(args);
            }
            catch (FormatException)
            {
                throw new ArgumentException("SetSubtitleStreamIndex: Invalid format, expecting a single integer argurment for SubtitleStreamIndex");
            }

            _playback.SetSubtitleStreamIndex(index);

        }
        
        // todo - move these to _presentation
        public void FullScreen(Object sender, CommandEventArgs args)
        {
            _presenation.MainApplicationWindow.WindowState = WindowState.Maximized;
            _presenation.EnsureApplicationWindowHasFocus();
        }

        public void MinimizeScreen(Object sender, CommandEventArgs args)
        {
            _presenation.MainApplicationWindow.WindowState = WindowState.Minimized;
            _presenation.EnsureApplicationWindowHasFocus();
        }

        public void RestoreScreen(Object sender, CommandEventArgs args)
        {
            _presenation.MainApplicationWindow.WindowState = WindowState.Normal;
            _presenation.EnsureApplicationWindowHasFocus();
        }

        public void ToggleFullscreen(Object sender, CommandEventArgs args)
        {
            if (_presenation.MainApplicationWindow.WindowState == WindowState.Maximized)
            {
                RestoreScreen(sender, args);
            }
            else
            {
                FullScreen(sender, args);
            }
        }

//        public void ShowScreensaver(Object sender, CommandEventArgs args)
//        {
//            _screensaverManager.ShowScreensaver(true);
//        }
    }
}