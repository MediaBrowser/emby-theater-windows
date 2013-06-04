using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Theming
{
    public interface ITheme
    {
        IEnumerable<ResourceDictionary> GetGlobalResources();

        Page GetLoginPage();

        Page GetInternalPlayerPage();

        Page GetHomePage();

        Page GetItemPage(string id, string type, string name, bool isFolder);
    }
}
