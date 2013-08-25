using MediaBrowser.Model.Dto;
using System.Windows;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IHomePage
    {
        string Name { get; }

        FrameworkElement GetHomePage(BaseItemDto rootFolder);
    }
}
