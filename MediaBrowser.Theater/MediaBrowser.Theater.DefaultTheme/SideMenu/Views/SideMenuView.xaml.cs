using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.Views
{
    /// <summary>
    ///     Interaction logic for SideMenuView.xaml
    /// </summary>
    public partial class SideMenuView : UserControl
    {
        public SideMenuView()
        {
            InitializeComponent();

            Sidebar.MouseDown += SideMenuView_MouseDown;
            Loaded += SideMenuView_Loaded;
        }

        void SideMenuView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void SideMenuView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}