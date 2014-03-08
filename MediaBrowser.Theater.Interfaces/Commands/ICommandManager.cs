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
        event CommandEventHandler CommandReceived;
    }
}
