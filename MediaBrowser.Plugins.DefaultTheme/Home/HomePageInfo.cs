using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class HomePageInfo : IHomePage
    {
        public string Name
        {
            get { return "Default"; }
        }

        public Type PageType
        {
            get { return typeof(HomePage); }
        }
    }
}
