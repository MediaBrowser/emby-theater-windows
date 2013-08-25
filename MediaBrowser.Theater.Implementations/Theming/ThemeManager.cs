using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Theater.Implementations.Theming
{
    public class ThemeManager : IThemeManager
    {
        private readonly Func<IPresentationManager> _presentationManager;

        public event EventHandler<EventArgs> ThemeLoaded;

        public ThemeManager(Func<IPresentationManager> presentationManager)
        {
            _presentationManager = presentationManager;
        }

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
                UnloadThemeInternal(_currentTheme);
                hadThemePrior = true;
            }

            LoadThemeInternal(theme);

            if (hadThemePrior)
            {
                // TODO: Figure out how to determine when this has completed
                await Task.Delay(10);
            }
        }

        private List<ResourceDictionary> _themeResources;

        private void LoadThemeInternal(ITheme theme)
        {
            _themeResources = theme.GetResources().ToList();

            var presentationManager = _presentationManager();

            foreach (var resource in _themeResources)
            {
                presentationManager.AddResourceDictionary(resource);
            }

            theme.Load();

            _currentTheme = theme;
        }

        private void UnloadThemeInternal(ITheme theme)
        {
            var presentationManager = _presentationManager();

            foreach (var resource in _themeResources)
            {
                presentationManager.RemoveResourceDictionary(resource);
            }

            _themeResources = null;
            
            theme.Unload();
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
