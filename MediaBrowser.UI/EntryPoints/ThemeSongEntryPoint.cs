using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.UI.EntryPoints
{
    class ThemeSongEntryPoint : IStartupEntryPoint, IDisposable
    {
        private INavigationService _nav;

        public void Run()
        {
        }

        public void Dispose()
        {
        }
    }
}
