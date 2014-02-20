using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for ManualLoginView.xaml
    /// </summary>
    public partial class ManualLoginView : UserControl
    {
        public ManualLoginView()
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