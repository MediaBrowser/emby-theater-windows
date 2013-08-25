using System;

namespace MediaBrowser.Theater.Interfaces.Navigation
{
    public class NavigationEventArgs : EventArgs
    {
        public object NewPage { get; set; }
        public object OldPage { get; set; }
    }
}
