using MediaBrowser.Model.Dto;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IHomePageInfo
    {
        string Name { get; }

        Page GetHomePage(BaseItemDto rootFolder);
    }

    public interface IHomePage
    {
        
    }
}
