using MediaBrowser.Theater.Api.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels
{
    public class SideMenuPath : NavigationPath { }

    public static class SideMenuPathExtensions
    {
        public static SideMenuPath SideMenu(this Go go)
        {
            return new SideMenuPath();
        }
    }
}
