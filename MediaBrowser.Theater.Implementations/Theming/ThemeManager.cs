using System.Threading.Tasks;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async void LoadTheme(ITheme theme)
        {
            _currentTheme = theme;

            var resources = _currentTheme.GetGlobalResources().ToList();

            foreach (var resource in resources)
            {
                Application.Current.Resources.MergedDictionaries.Add(resource);
            }

            //await Task.Delay(5000);

            //var current = Application.Current.Resources.MergedDictionaries.ToList();

            //Application.Current.Resources.MergedDictionaries.Clear();

            //foreach (var resource in current.Where(i => !resources.Contains(i)))
            //{
            //    Application.Current.Resources.MergedDictionaries.Add(resource);
            //}
        }

        public void LoadDefaultTheme()
        {
            LoadTheme(Themes.First(i => string.Equals(i.Name, "Default")));
        }
    }
}
