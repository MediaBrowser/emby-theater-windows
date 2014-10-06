using System;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.EntryPoints.CommandActions
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

            var focused = Keyboard.FocusedElement;
            if (focused != null) {
                var handled = SendRoutedEvent(focused, InputCommands.PreviewCommandSentEvent, commandEventArgs.Command, commandEventArgs.Args) ||
                              SendRoutedEvent(focused, InputCommands.CommandSentEvent, commandEventArgs.Command, commandEventArgs.Args);

                if (handled) {
                    commandEventArgs.Handled = true;
                    return;
                }
            }

            commandEventArgs.Handled = _defaultCommandActionMap.ExecuteCommand(commandEventArgs.Command, commandEventArgs.Args);
        }

        private bool SendRoutedEvent(IInputElement element, RoutedEvent routedEvent, Command command, object arg)
        {
            var routedEventArgs = new CommandRoutedEventArgs(routedEvent) { Command = command, Args = arg };
            element.RaiseEvent(routedEventArgs);

            return routedEventArgs.Handled;
        }

        public void Dispose()
        {
            _commandManager.CommandReceived -= commandManager_CommandReceived;
        }
    }
}



