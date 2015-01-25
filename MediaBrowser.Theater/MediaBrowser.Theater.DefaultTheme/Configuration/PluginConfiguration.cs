using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.Configuration
{
    public class SortModePreference
    {
        public string ItemType {get;set;}
        public string SortModeType { get; set; }
        public SortDirection SortDirection { get; set; }
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

        public List<SortModePreference> SortModes { get; set; }

        public PluginConfiguration()
        {
            Palette = new ColorPalette { Style = ThemeStyle.Light, Accent = AccentColors.MediaBrowserGreen };
            SortModes = new List<SortModePreference>();
            ShowOnlyWatchedChapters = false;
        }

        public SortModePreference FindSortMode(string itemType)
        {
            return SortModes.FirstOrDefault(sm => sm.ItemType == itemType);
        }

        public void SaveSortMode(string itemType, IItemListSortMode sortMode, SortDirection direction)
        {
            var preference = SortModes.FirstOrDefault(sm => sm.ItemType == itemType);
            if (preference != null) {
                preference.SortModeType = sortMode.GetType().FullName;
                preference.SortDirection = direction;
            } else {
                SortModes.Add(new SortModePreference { ItemType = itemType, SortModeType = sortMode.GetType().FullName, SortDirection = direction });
            }

            Theme.Instance.SaveConfiguration();
        }
    }
}