using System.Collections.Generic;
using System.Windows;
using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Theater.DefaultTheme.Configuration
{
    public class SortModePreference
    {
        public string ItemType {get;set;}
        public string SortModeType { get; set; }
    }

    public class PluginConfiguration
        : BasePluginConfiguration
    {
        public ColorPalette Palette { get; set; }

        public WindowState? WindowState { get; set; }

        public double? WindowTop { get; set; }

        public double? WindowLeft { get; set; }

        public double? WindowWidth { get; set; }

        public double? WindowHeight { get; set; }

        public bool ShowOnlyWatchedChapters { get; set; }

        public SortModePreference[] SortModes { get; set; }

        public PluginConfiguration()
        {
            Palette = new ColorPalette { Style = ThemeStyle.Light, Accent = AccentColors.MediaBrowserGreen };
            SortModes = new SortModePreference[0];
            ShowOnlyWatchedChapters = true;
        }
    }
}