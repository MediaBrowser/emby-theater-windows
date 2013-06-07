using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Windows;

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

        public void SetCurrentTheme(ITheme theme)
        {
            _currentTheme = theme;

            foreach (var resource in _currentTheme.GetGlobalResources())
            {
                Application.Current.Resources.MergedDictionaries.Add(resource);
            }
        }
    }
}
