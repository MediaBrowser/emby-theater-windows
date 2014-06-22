using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.SortModeMenu.Views
{
    /// <summary>
    ///     Interaction logic for SortModeMenuView.xaml
    /// </summary>
    public partial class SortModeMenuView
    {
        public SortModeMenuView()
        {
            InitializeComponent();

            Sidebar.MouseDown += Sidebar_MouseDown;
            Loaded += SortModeMenuView_Loaded;
        }

        private void SortModeMenuView_Loaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void Sidebar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}