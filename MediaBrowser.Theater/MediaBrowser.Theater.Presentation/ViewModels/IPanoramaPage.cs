using System.ComponentModel;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public interface IPanoramaPage : INotifyPropertyChanged
    {
        string DisplayName { get; }
        bool IsTitlePage { get; }
        bool IsVisible { get; }
    }
}