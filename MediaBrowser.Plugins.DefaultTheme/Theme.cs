using MediaBrowser.Plugins.DefaultTheme.Pages;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Theming;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme
{
    /// <summary>
    /// Class Theme
    /// </summary>
    class Theme : ITheme
    {
        public IEnumerable<ResourceDictionary> GetGlobalResources()
        {
            return new[] { new AppResources() };
        }

        public Page GetLoginPage()
        {
            return new LoginPage();
        }

        public Page GetInternalPlayerPage()
        {
            return new InternalPlayerPage();
        }

        public Page GetHomePage()
        {
            return new HomePage();
        }

        public Page GetItemPage(string id, string type, string name, bool isFolder)
        {
            if (isFolder)
            {
                return new ListPage(id);
            }

            return new DetailPage(id);
        }
    }
}
