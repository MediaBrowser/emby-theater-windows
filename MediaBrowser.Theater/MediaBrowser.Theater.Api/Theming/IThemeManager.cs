using System.Collections.Generic;
using System.Reflection;

namespace MediaBrowser.Theater.Api.Theming
{
    public interface IThemeManager
    {
        IEnumerable<ThemeInfo> FindInstalledThemes();
        Assembly LoadSelectedTheme();
    }
}