using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.Views
{
    /// <summary>
    ///     Interaction logic for SideMenuView.xaml
    /// </summary>
    public partial class SideMenuView
    {
        public SideMenuView()
        {
            InitializeComponent();

            Sidebar.MouseDown += SideMenuView_MouseDown;
            Loaded += SideMenuView_Loaded;
        }

        private void SideMenuView_Loaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void SideMenuView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}