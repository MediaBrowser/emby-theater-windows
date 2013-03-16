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
            Title = _appHost.ApplicationVersion.ToString();
        }
    }
}
