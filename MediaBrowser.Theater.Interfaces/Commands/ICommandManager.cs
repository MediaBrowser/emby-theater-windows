using System;
using System.Windows;

namespace MediaBrowser.Theater.Interfaces.Commands
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
        /// subscribe to commands. It up to teh subscribe to decide how to exeute teh command.
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
}
