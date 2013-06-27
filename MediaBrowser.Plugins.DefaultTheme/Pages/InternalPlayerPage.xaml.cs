using MediaBrowser.Plugins.DefaultTheme.Resources;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Presentation.Pages;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for InternalPlayerPage.xaml
    /// </summary>
    public partial class InternalPlayerPage : BasePage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalPlayerPage"/> class.
        /// </summary>
        public InternalPlayerPage()
        {
            InitializeComponent();

            Loaded += InternalPlayerPage_Loaded;
            Unloaded += InternalPlayerPage_Unloaded;
        }

        /// <summary>
        /// Handles the Unloaded event of the InternalPlayerPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void InternalPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AppResources.Instance.HeaderContent.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Loaded event of the InternalPlayerPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void InternalPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            AppResources.Instance.ClearPageTitle();
            AppResources.Instance.HeaderContent.Visibility = Visibility.Collapsed;
        }
    }
}
