using System;
using System.Windows;
using System.Windows.Media;

namespace MediaBrowser.Theater.DefaultTheme.Configuration
{
    public enum ThemeStyle
    {
        Light,
        Dark
    }

    public class ColorPalette
    {
        public ThemeStyle Style { get; set; }
        public Color Accent { get; set; }

        public virtual ResourceDictionary GetResources()
        {
            var uri = (Style == ThemeStyle.Dark) ? 
                "/MediaBrowser.Theater.DefaultTheme;component/Resources/Styles/DarkColors.xaml" : 
                "/MediaBrowser.Theater.DefaultTheme;component/Resources/Styles/LightColors.xaml";

            var resources = new ResourceDictionary { Source = new Uri(uri, UriKind.RelativeOrAbsolute) };
            resources["AccentColor"] = Accent;

            return resources;
        }
    }
}