using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Commands;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using WindowsInput = System.Windows.Input;

 namespace MediaBrowser.UI.EntryPoints.CommandActions
{

    public class CommandActionsEntryPoint :  IStartupEntryPoint, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICommandManager _commandManager;
        private readonly DefaultCommandActionMap _defaultCommandActionMap;


        public CommandActionsEntryPoint(ICommandManager commandManager, IPresentationManager presentationManager, IPlaybackManager playbackManager, INavigationService navigationService, IScreensaverManager screensaverManager, ILogManager logManager)
        {
            _commandManager = commandManager;
            _defaultCommandActionMap = new DefaultCommandActionMap(presentationManager, playbackManager, navigationService, screensaverManager, logManager);
        
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



