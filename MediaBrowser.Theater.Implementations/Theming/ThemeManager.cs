using MediaBrowser.Theater.Interfaces.Theming;
using System.Collections.Generic;

namespace MediaBrowser.Theater.Implementations.Theming
{
    public class ThemeManager : IThemeManager
    {
        public void AddParts(IEnumerable<ITheme> themes)
        {
            _themes.AddRange(themes);
        }

        private readonly List<ITheme> _themes = new List<ITheme>();
        public IEnumerable<ITheme> Themes
        {
            get { return _themes; }
        }

        private ITheme _currentTheme;
        public ITheme CurrentTheme
        {
            get { return _currentTheme; }
        }

        public void SetCurrentTheme(ITheme theme)
        {
            _currentTheme = theme;
        }
    }
}
