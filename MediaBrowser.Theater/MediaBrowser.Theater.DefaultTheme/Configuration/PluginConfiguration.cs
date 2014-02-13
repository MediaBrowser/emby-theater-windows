using System.Windows;
using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Theater.DefaultTheme.Configuration
{
    public class PluginConfiguration
        : BasePluginConfiguration
    {
        public ColorPalette Palette { get; set; }

        public WindowState? WindowState { get; set; }

        public double? WindowTop { get; set; }

        public double? WindowLeft { get; set; }

        public double? WindowWidth { get; set; }

        public double? WindowHeight { get; set; }

        public PluginConfiguration()
        {
            Palette = new ColorPalette { Style = ThemeStyle.Dark, Accent = AccentColors.MediaBrowserGreen };
        }
    }
}