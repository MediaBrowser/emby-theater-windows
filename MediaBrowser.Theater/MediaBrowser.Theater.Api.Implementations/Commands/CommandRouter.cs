using System;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Api.Commands
{
    public class CommandRouter
        : ICommandRouter
    {
        private readonly DefaultCommandActionMap _defaultCommandActionMap;
        private readonly ILogger _logger;

        public CommandRouter(ITheaterApplicationHost appHost, IPresenter presentationManager, IPlaybackManager playbackManager, INavigator navigationService, ILogManager logManager, IEventAggregator events)
        {
            _defaultCommandActionMap = new DefaultCommandActionMap(appHost, presentationManager, playbackManager, navigationService, logManager, events);

            _logger = logManager.GetLogger(GetType().Name);
        }

        public event CommandEventHandler CommandReceived;

        protected virtual void OnCommandReceived(CommandEventArgs commandeventargs)
        {
            CommandEventHandler handler = CommandReceived;
            if (handler != null) {
                handler(this, commandeventargs);
            }
        }

        public bool RouteCommand(Command command, object args)
        {
            Func<bool> action = () => {
                _logger.Debug("Command received {0} {1}", command, args);

                IInputElement focused = Keyboard.FocusedElement;
                if (focused != null) {
                    bool handled = SendRoutedEvent(focused, InputCommands.PreviewCommandSentEvent, command, args) ||
                                   SendRoutedEvent(focused, InputCommands.CommandSentEvent, command, args);

                    if (handled) {
                        return true;
                    }
                }

                var commandEvent = new CommandEventArgs { Command = command, Args = args };
                OnCommandReceived(commandEvent);

                if (commandEvent.Handled) {
                    return true;
                }

                return _defaultCommandActionMap.ExecuteCommand(command, args);
            };

            return action.OnUiThreadAsync().Result;
        }

        private bool SendRoutedEvent(IInputElement element, RoutedEvent routedEvent, Command command, object arg)
        {
            var routedEventArgs = new CommandRoutedEventArgs(routedEvent) { Command = command, Args = arg };
            element.RaiseEvent(routedEventArgs);

            return routedEventArgs.Handled;
        }
    }
}