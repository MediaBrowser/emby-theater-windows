using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferencesMenu
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : BasePage
    {
        private readonly DisplayPreferencesMenu _window;
        private readonly DisplayPreferencesViewModel _displayPreferencesViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage(DisplayPreferencesMenu window, DisplayPreferencesViewModel displayPreferencesViewModel)
        {
            _window = window;
            _displayPreferencesViewModel = displayPreferencesViewModel;
            InitializeComponent();

            ViewMenuButton.Click += ViewMenuButton_Click;
            SortMenuButton.Click += SortMenuButton_Click;

            DataContext = _displayPreferencesViewModel;
        }

        /// <summary>
        /// Handles the Click event of the SortMenuButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void SortMenuButton_Click(object sender, RoutedEventArgs e)
        {
            _window.NavigateToSortMenu();
        }

        /// <summary>
        /// Handles the Click event of the ViewMenuButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void ViewMenuButton_Click(object sender, RoutedEventArgs e)
        {
            _window.NavigateToViewMenu();
        }
    }
}
