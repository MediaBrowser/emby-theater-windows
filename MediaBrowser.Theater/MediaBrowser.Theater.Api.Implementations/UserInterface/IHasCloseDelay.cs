using System;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IHasCloseDelay
    {
        TimeSpan CloseDelay { get; set; }
    }
}