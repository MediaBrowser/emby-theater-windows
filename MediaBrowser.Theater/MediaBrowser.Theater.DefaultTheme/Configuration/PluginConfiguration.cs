using System.Windows.Media;
using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Theater.DefaultTheme.Configuration
{
    public class PluginConfiguration
        : BasePluginConfiguration
    {
        public ColorPalette Palette { get; set; }

        public PluginConfiguration()
        {
            Palette = new ColorPalette { Style = ThemeStyle.Dark, Accent = Color.FromRgb(82, 181, 75) };
        }
    }
}