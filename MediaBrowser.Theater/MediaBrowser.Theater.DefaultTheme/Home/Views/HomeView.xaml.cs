using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.Home.Views
{
    /// <summary>
    ///     Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            Loaded += (s, e) => {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            };
        }
    }
}