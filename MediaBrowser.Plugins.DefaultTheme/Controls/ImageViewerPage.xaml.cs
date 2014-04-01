using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Pages;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for ImageViewerPage.xaml
    /// </summary>
    public partial class ImageViewerPage : BasePage, ISupportsThemeMedia
    {
        private readonly IPresentationManager _presentation;

        public ImageViewerPage(IPresentationManager presentation, ImageViewerViewModel viewModel)
        {
            _presentation = presentation;
            InitializeComponent();

            DataContext = viewModel;

            Loaded += ImageViewerPage_Loaded;
            Unloaded += ImageViewerPage_Unloaded;
        }

        void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetGlobalThemeContentVisibility(true);
        }

        void ImageViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetGlobalThemeContentVisibility(false);
        }
    }
}
