using System;
using System.Windows;

namespace MediaBrowser.Theater.Api.Commands
{
    // Inheriet of routedEventArgs, but for now treat as plan event arges
    // Later implement our own routing, notr based on UI tree, to manage Handled 
    public class CommandEventArgs : EventArgs //.. RoutedEventArgs 
    {
        public Command Command;
        public Object Args;             // page name, Key, list of widgetsg
        public Boolean Handled;
    }
    
    public delegate void CommandEventHandler(object sender, CommandEventArgs commandEventArgs);

    public interface ICommandManager
    {
        /// <summary>
        /// subscribe to commands. It up to teh subscribe to decide how to exeute the command.
        /// if  the subscribe executes the command they should set handed to true
        /// </summary>
        event CommandEventHandler CommandReceived;

        /// <summary>
        /// Send a command to command subsribers
        ///  </summary>
        /// <param name="command">The command to send</param>
        /// <param name="args">The command arguments</param>
        bool ExecuteCommand(Command command, Object args);
    }

    public interface ICommandRouter
    {
        event CommandEventHandler CommandReceived;
        bool RouteCommand(Command command, Object args);
    }

    public class CommandRoutedEventArgs : RoutedEventArgs
    {
        public Command Command { get;set;}
        public object Args { get; set; }

        public CommandRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }

    public delegate void CommandRoutedEventHandler(object sender, CommandRoutedEventArgs e);

    public class InputCommands
    {
        public static readonly RoutedEvent PreviewCommandSentEvent = EventManager.RegisterRoutedEvent("PreviewCommandSent", RoutingStrategy.Tunnel,
                                                                                               typeof(RoutedEventHandler), typeof(InputCommands));

        public static readonly RoutedEvent CommandSentEvent = EventManager.RegisterRoutedEvent("CommandSent", RoutingStrategy.Bubble,
                                                                                               typeof(RoutedEventHandler), typeof(InputCommands));

        public static void AddCommandSentEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie != null)
            {
                uie.AddHandler(CommandSentEvent, handler);
            }
        }

        public static void RemoveCommandSentEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(CommandSentEvent, handler);
            }
        }
    }
}
