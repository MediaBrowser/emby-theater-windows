using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            Loaded += (s, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
    }
}