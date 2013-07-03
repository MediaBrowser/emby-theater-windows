using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.Theming
{
    public class ThemeManager : IThemeManager
    {
        public event EventHandler<EventArgs> ThemeLoaded;

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

        public async Task LoadTheme(ITheme theme)
        {
            if (_currentTheme == theme)
            {
                return;
            }

            var hadThemePrior = false;

            if (_currentTheme != null)
            {
                _currentTheme.Unload();
                hadThemePrior = true;
            }

            _currentTheme = theme;

            _currentTheme.Load();

            if (hadThemePrior)
            {
                // TODO: Figure out how to determine when this has completed
                await Task.Delay(10);
            }
        }

        public Task LoadDefaultTheme()
        {
            return LoadTheme(DefaultTheme);
        }

        public ITheme DefaultTheme
        {
            get { return Themes.First(i => string.Equals(i.Name, "Default")); }
        }
    }
}
