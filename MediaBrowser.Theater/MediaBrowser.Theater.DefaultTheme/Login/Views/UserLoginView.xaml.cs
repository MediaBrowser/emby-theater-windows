using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for UserLoginView.xaml
    /// </summary>
    public partial class UserLoginView : UserControl
    {
        public UserLoginView()
        {
            InitializeComponent();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            base.OnMouseDown(e);
        }
    }
}