using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using System;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.EntryPoints.CommandActions;
using WindowsInput = System.Windows.Input;

 namespace MediaBrowser.UI.EntryPoints.CommandActions
{

    public class CommandActionsEntryPoint :  IStartupEntryPoint, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICommandManager _commandManager;
        private readonly DefaultCommandActionMap _defaultCommandActionMap;


        public CommandActionsEntryPoint(ICommandManager commandManager, ITheaterApplicationHost appHost, IPresenter presentationManager, IPlaybackManager playbackManager, INavigator navigationService, /*IScreensaverManager screensaverManager,*/ ILogManager logManager, IEventAggregator events)
        {
            _commandManager = commandManager;
            _defaultCommandActionMap = new DefaultCommandActionMap(appHost, presentationManager, playbackManager, navigationService, /*screensaverManager,*/ logManager, events);
        
            _logger = logManager.GetLogger(GetType().Name);
        }

        public void Run()
        {
            _commandManager.CommandReceived += commandManager_CommandReceived;
        }

        private void commandManager_CommandReceived(object sender, CommandEventArgs commandEventArgs)
        {
            _logger.Debug("commandManager_CommandReceived {0} {1}", commandEventArgs.Command, commandEventArgs.Args);
            commandEventArgs.Handled = _defaultCommandActionMap.ExecuteCommand(commandEventArgs.Command, commandEventArgs.Args);
        }

        public void Dispose()
        {
            _commandManager.CommandReceived -= commandManager_CommandReceived;
        }
    }
}



