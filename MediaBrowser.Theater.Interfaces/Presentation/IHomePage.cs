using System;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IHomePage
    {
        string Name { get; }

        Type PageType { get; }
    }
}
