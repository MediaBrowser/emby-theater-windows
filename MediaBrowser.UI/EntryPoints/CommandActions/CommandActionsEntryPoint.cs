﻿using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Implementations.CommandActions;
using MediaBrowser.Theater.Interfaces.Commands;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using WindowsInput = System.Windows.Input;

 namespace MediaBrowser.UI.EntryPoints
{

    public class CommandActionsEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly ILogger _logger;
        private ICommandManager _commandManager;
        private readonly DefaultCommandActionMap _defaultCommandActionMap;


        public CommandActionsEntryPoint(ICommandManager commandManager, IPresentationManager presentationManager, IPlaybackManager playbackManager, INavigationService navigationService, ILogManager logManager)
        {
            _commandManager = commandManager;
            _defaultCommandActionMap = new DefaultCommandActionMap(presentationManager, playbackManager, navigationService, logManager);
        
            _logger = logManager.GetLogger(GetType().Name);
        }

        public void Run()
        {
            _commandManager.CommandReceived += commandManager_CommandReceived;
        }

        private void commandManager_CommandReceived(object sender, CommandEventArgs commandEventArgs)
        {
            if (commandEventArgs.Command == Command.Null)
                return;

            _logger.Debug("commandManager_CommandReceived {0} {1}", commandEventArgs.Command, commandEventArgs.Args);
            commandEventArgs.Handled = _defaultCommandActionMap.ExecuteCommand(commandEventArgs.Command);
        }

        public void Dispose()
        {
            _commandManager.CommandReceived -= commandManager_CommandReceived;
        }
    }
}



