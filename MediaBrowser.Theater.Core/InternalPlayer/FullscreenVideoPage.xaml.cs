using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System.Windows;

namespace MediaBrowser.Theater.Core.InternalPlayer
{
    /// <summary>
    /// Interaction logic for FullscreenVideoPage.xaml
    /// </summary>
    public partial class FullscreenVideoPage : BasePage, ISupportsThemeMedia, IFullscreenVideoPage
    {
        private readonly IThemeManager _themeManager;

        public FullscreenVideoPage(IThemeManager themeManager)
        {
            _themeManager = themeManager;
            InitializeComponent();

            Loaded += FullscreenVideoPage_Loaded;
            Unloaded += FullscreenVideoPage_Unloaded;
        }

        void FullscreenVideoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _themeManager.CurrentTheme.SetGlobalContentVisibility(true);
        }

        void FullscreenVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _themeManager.CurrentTheme.SetGlobalContentVisibility(false);
        }
    }
}
