using System;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public interface IHasCloseDelay
    {
        TimeSpan CloseDelay { get; set; }
    }
}