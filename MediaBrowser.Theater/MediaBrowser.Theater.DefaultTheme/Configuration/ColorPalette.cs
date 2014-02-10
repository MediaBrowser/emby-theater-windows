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
        public static Color DarkBackground
        {
            get { return Color.FromRgb(30, 30, 30); }
        }

        public static Color LightBackground
        {
            get { return Colors.White; }
        }

        public static Color DarkForeground
        {
            get { return Colors.White; }
        }

        public static Color LightForeground
        {
            get { return Colors.Black; }
        }

        public ThemeStyle Style { get; set; }
        public Color Accent { get; set; }
    }
}