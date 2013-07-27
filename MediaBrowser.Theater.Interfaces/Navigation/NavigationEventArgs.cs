using System;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Navigation
{
    public class NavigationEventArgs : EventArgs
    {
        public Page NewPage { get; set; }
        public Page OldPage { get; set; }
    }
}
