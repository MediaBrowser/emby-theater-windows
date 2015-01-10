using System.Windows.Controls;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for WelcomePageView.xaml
    /// </summary>
    public partial class WelcomePageView : UserControl
    {
        public WelcomePageView()
        {
            InitializeComponent();

            Loaded += (s, e) => LoginConnectButton.Focus();
        }
    }
}