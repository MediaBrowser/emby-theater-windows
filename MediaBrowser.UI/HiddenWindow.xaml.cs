using System.Windows;
using MediaBrowser.Common;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Interaction logic for HiddenWindow.xaml
    /// </summary>
    public partial class HiddenWindow : Window
    {
        private IApplicationHost _appHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenWindow" /> class.
        /// </summary>
        public HiddenWindow(IApplicationHost appHost)
        {
            _appHost = appHost;
            InitializeComponent();

            Loaded += HiddenWindow_Loaded;
        }

        void HiddenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Show the version number for now until we find a better place (in tools/settings area)
            Title = "Media Browser " + _appHost.ApplicationVersion;
        }
    }
}
