using MediaBrowser.Model.Dto;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IHomePage
    {
        string Name { get; }

        Page GetHomePage(BaseItemDto rootFolder);
    }
}
