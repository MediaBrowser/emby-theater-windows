using System.ComponentModel;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public interface IRootPresentationOptions : INotifyPropertyChanged
    {
        bool ShowMediaBrowserLogo { get; }
        bool ShowCommandBar { get; }
    }
}