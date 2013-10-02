using MediaBrowser.Theater.Presentation.Controls;
using System.Windows;

namespace MediaBrowser.Theater.Core.FullscreenVideo
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : BaseModalWindow
    {
        public InfoWindow()
        {
            InitializeComponent();

            MainGrid.DataContext = this;

            Loaded += InfoWindow_Loaded;
        }

        void InfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InfoPanel.DataContext = DataContext;
        }
    }
}
