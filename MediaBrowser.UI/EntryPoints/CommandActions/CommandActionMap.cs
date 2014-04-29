using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Commands;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Playback;


 namespace MediaBrowser.UI.EntryPoints.CommandActions
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
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly INavigationService _navigationService;
        private readonly IScreensaverManager _screensaverManager;
        private readonly ILogger _logger;
        private readonly CommandActionMapping _nullCommandActionMapping;
        private readonly CommandActionMap _globalCommandActionMap;
        private double _currentPlaybackRate = 1.0; // Todo - move to reportable property of IPlaybackManager

        public DefaultCommandActionMap(IPresentationManager presentationManager, IPlaybackManager playbackManager, INavigationService navigationService, IScreensaverManager screensaverManager, ILogManager logManager)
        {
            _presentationManager = presentationManager;
            _playbackManager = playbackManager;
            _screensaverManager = screensaverManager;
            _navigationService = navigationService;
            _logger = logManager.GetLogger(GetType().Name);
            _globalCommandActionMap = CreateGlobalCommandActionMap();
            _nullCommandActionMapping = new CommandActionMapping(Command.Null, NullAction);

            _playbackManager.PlaybackStarted += PlaybackManagerPlaybackManagerStarted;
            _playbackManager.PlaybackCompleted+=PlaybackManagerPlaybackManagerCompleted;
        }

        private void PlaybackManagerPlaybackManagerStarted(object sender, PlaybackStartEventArgs playbackStartEventArgs)
        {
            _currentPlaybackRate = 1.0;
        }

        private void PlaybackManagerPlaybackManagerCompleted(object sender, PlaybackStopEventArgs e)
        {
            _currentPlaybackRate = 1.0;
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
                new CommandActionMapping( Command.Back,            (s, a) => _navigationService.NavigateBack()),
                new CommandActionMapping( Command.Forward,         (s, a) => _navigationService.NavigateForward()),
                new CommandActionMapping( Command.GotoHome,        (s, a) => _navigationService.NavigateToHomePage()),
                new CommandActionMapping( Command.GotoSearch,      (s, a) => _navigationService.NavigateToSearchPage()),
                new CommandActionMapping( Command.GotoSettings,    (s, a) => _navigationService.NavigateToSettingsPage()),
                new CommandActionMapping( Command.GotoPage,        NullAction),
                new CommandActionMapping( Command.Info,            ToggleInfoPanel),
                new CommandActionMapping( Command.SkipNext,        SkipForward,        60),    // skip forward 60  seconds, boxed arguments
                new CommandActionMapping( Command.SkipPrevious,    SkipBackward,       60),
                new CommandActionMapping( Command.Step,            SkipForward,        60),     
                new CommandActionMapping( Command.SmallStepForward,SkipForward,        10),
                new CommandActionMapping( Command.SmallStepBack,   SkipBackward,       10),
                new CommandActionMapping( Command.StepForward,     SkipBackward,       60),
                new CommandActionMapping( Command.StepBack,        SkipBackward,       60),
                new CommandActionMapping( Command.BigStepForward,  SkipForward,        300),
                new CommandActionMapping( Command.BigStepBack,     SkipBackward,       300),
                new CommandActionMapping( Command.FullScreen,               (s,a) => _presentationManager.FullScreen()),
                new CommandActionMapping( Command.MinimizeScreen,           (s,a) => _presentationManager.MinimizeScreen()),
                new CommandActionMapping( Command.RestoreScreen,            (s,a) => _presentationManager.RestoreScreen()),
                new CommandActionMapping( Command.ToggleFullScreen,         (s,a) => _presentationManager.ToggleFullscreen()),
                new CommandActionMapping( Command.SetVolume,                SetVolume),
                new CommandActionMapping( Command.VolumeUp,                 (s,e) => _playbackManager.VolumeStepUp()),
                new CommandActionMapping( Command.VolumeDown,               (s,e) => _playbackManager.VolumeStepDown()),
                new CommandActionMapping( Command.Mute,                     (s,e) => _playbackManager.Mute()),
                new CommandActionMapping( Command.UnMute,                   (s,e) => _playbackManager.UnMute()),
                new CommandActionMapping( Command.ToggleMute,               (s,e) => _playbackManager.ToggleMute()),
                new CommandActionMapping( Command.SetSubtitleStreamIndex,   SetSubtitleStreamIndex),
                new CommandActionMapping( Command.NextSubtitleStream,       (s, a) => _playbackManager.NextSubtitleStream()),
                new CommandActionMapping( Command.SetAudioStreamIndex,      SetAudioStreamIndex),
                new CommandActionMapping( Command.NextAudioStream,          (s, a) => _playbackManager.NextAudioStream()),
                new CommandActionMapping( Command.AspectRatio,              NullAction), // ToDo
                new CommandActionMapping( Command.ShowOsd,                  ShowOsd),
                new CommandActionMapping( Command.HideOsd,                  HideOSd),
                new CommandActionMapping( Command.ToggleOsd,                ToggleOsd),
                new CommandActionMapping( Command.ShowInfoPanel,            ShowInfoPanel),
                new CommandActionMapping( Command.HideinfoPanel,            HideInfoPanel),
                new CommandActionMapping( Command.ToggleInfoPanel,          ToggleInfoPanel),
                new CommandActionMapping( Command.ShowScreensaver,          (s, a) => _screensaverManager.ShowScreensaver(true)),
                new CommandActionMapping( Command.ScreenDump,               (s, a) => MBTScreenDump.GetAndSaveWindowsImage(_presentationManager.WindowHandle)),

            };
        }

        private CommandActionMapping MapCommand(Command command)
        {
            return _globalCommandActionMap.FirstOrDefault(a => a.Command == command) ?? _nullCommandActionMapping;
        }

        public Boolean ExecuteCommand(Command command)
        {
            var commandAction = MapCommand(command);
            return ExecuteCommandAction(commandAction);
        }

        private Boolean ExecuteCommandAction(CommandActionMapping commandActionMapping)
        {
            _logger.Debug("ExecuteCommandAction {0} {1}", commandActionMapping.Command, commandActionMapping.Args);
            var handled = false;

            try
            {
                var commandEventArgs = new CommandEventArgs()
                {
                    Args = commandActionMapping.Args,
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

        private IInternalMediaPlayer GetActiveInternalMediaPlayer()
        {
            return _playbackManager.MediaPlayers.OfType<IInternalMediaPlayer>().FirstOrDefault(i => i.PlayState != PlayState.Idle);
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

                ShowOsd(sender, args);
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

                ShowOsd(sender, args);
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
                ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void UnPause(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null && activePlayer.PlayState == PlayState.Paused)
            {
                activePlayer.UnPause();
                ShowOsd(sender, args);
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

                ShowOsd(sender, args);
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
                ShowOsd(sender, args);
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
                ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void SendPlayCommandToPresentation()
        {
            _presentationManager.Window.Dispatcher.InvokeAsync(() =>
            {
                var currentPage = _navigationService.CurrentPage;

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
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.NextTrack();
                ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void PreviousTrack(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.PreviousTrack();
                ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void NextChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.GoToNextChapter();
                ShowOsd(sender, args);
            }
            args.Handled = true;
        }

        private void PreviousChapter(Object sender, CommandEventArgs args)
        {
            var activePlayer = GetActiveInternalMediaPlayer();

            if (activePlayer != null)
            {
                activePlayer.GoToPreviousChapter();
                ShowOsd(sender, args);
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
            var activePlayer = GetActiveInternalMediaPlayer();

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
            _logger.Debug("Seek  {0}", args);

            long position;

            if (args == null)
                throw new ArgumentException("sender: expecting a single long argument");

            try
            {
                position = (long)Convert.ToUInt64(args);
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


        private void ShowOsd(Object sender, CommandEventArgs args)
        {
            if (GetActiveInternalMediaPlayer() != null)
            {
                _presentationManager.Window.Dispatcher.InvokeAsync(() =>
                {
                    var page = _navigationService.CurrentPage as IFullscreenVideoPage;

                    if (page != null)
                    {
                        page.ShowOsd();
                    }
               });
            }
        }


        private void HideOSd(Object sender, CommandEventArgs args)
        {
            if (GetActiveInternalMediaPlayer() != null)
            {
                _presentationManager.Window.Dispatcher.InvokeAsync(() =>
                {
                    var page = _navigationService.CurrentPage as IFullscreenVideoPage;

                    if (page != null)
                    {
                        page.HideOsd();
                    }
               });
            }
        }

        private void ToggleOsd(Object sender, CommandEventArgs args)
        {
            if (GetActiveInternalMediaPlayer() != null)
            {
                _presentationManager.Window.Dispatcher.InvokeAsync(() =>
                {
                    var page = _navigationService.CurrentPage as IFullscreenVideoPage;

                    if (page != null)
                    {
                        page.ToggleOsd();
                    }
                });
            }
        }

        private void ShowInfoPanel(Object sender, CommandEventArgs args)
        {
            if (GetActiveInternalMediaPlayer() != null)
            {
                _presentationManager.Window.Dispatcher.InvokeAsync(() =>
                {
                    var page = _navigationService.CurrentPage as IFullscreenVideoPage;

                    if (page != null)
                    {
                        page.ShowInfoPanel();
                    }
                });
            }
        }


        private void HideInfoPanel(Object sender, CommandEventArgs args)
        {
            if (GetActiveInternalMediaPlayer() != null)
            {
                _presentationManager.Window.Dispatcher.InvokeAsync(() =>
                {
                    var page = _navigationService.CurrentPage as IFullscreenVideoPage;

                    if (page != null)
                    {
                        page.HideInfoPanel();
                    }
                });
            }
        }

        private void ToggleInfoPanel(Object sender, CommandEventArgs args)
        {
            if (GetActiveInternalMediaPlayer() != null)
            {
                _presentationManager.Window.Dispatcher.InvokeAsync(() =>
                {
                    var page = _navigationService.CurrentPage as IFullscreenVideoPage;

                    if (page != null)
                    {
                        page.ToggleInfoPanel();
                    }
                });
            }
        }
      
        private void GotoHome(Object sender, CommandEventArgs args)
        {
            _navigationService.NavigateToHomePage();
            args.Handled = true;
        }

        private void GotoSettings(Object sender, CommandEventArgs args)
        {
            _navigationService.NavigateToSettingsPage();
            args.Handled = true;
        }

        private void GotoSearch(Object sender, CommandEventArgs args)
        {
            _navigationService.NavigateToSearchPage();
            args.Handled = true;
        }

       
        void SetVolume(Object sender, CommandEventArgs args)
        {
            _logger.Debug("SetVolume  {0}", args);

            float volume;

            if (args == null )
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

            _playbackManager.SetVolume(volume);
        }

        void SetAudioStreamIndex(Object sender, CommandEventArgs args)
        {
            _logger.Debug("SetAudioStreamIndex {0}", args);

            int index;

            if (args == null )
                throw new ArgumentException("SetAudioStreamIndex: expecting a single integer argurment for AudiostreamIndex");

            try
            {
                index = Convert.ToInt32(args);
            }
            catch (FormatException)
            {
                throw new ArgumentException("SetAudioStreamIndex: Invalid format, expecting a single integer argurment for AudiostreamIndex");
            }


            _playbackManager.SetAudioStreamIndex(index);
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

            _playbackManager.SetSubtitleStreamIndex(index);

        }
    }
}