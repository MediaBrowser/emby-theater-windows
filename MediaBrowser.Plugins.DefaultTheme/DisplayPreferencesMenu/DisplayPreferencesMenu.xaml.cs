using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferencesMenu
{
    /// <summary>
    /// Interaction logic for DisplayPreferencesMenu.xaml
    /// </summary>
    public partial class DisplayPreferencesMenu : BaseModalWindow
    {
        private readonly DisplayPreferencesViewModel _displayPreferencesViewModel;

        public DisplayPreferencesMenu(DisplayPreferencesViewModel displayPreferencesViewModel)
        {
            _displayPreferencesViewModel = displayPreferencesViewModel;

            InitializeComponent();

            btnClose.Click += btnClose_Click;
            Loaded += DisplayPreferencesMenu_Loaded;

            DataContext = this;
        }

        void DisplayPreferencesMenu_Loaded(object sender, RoutedEventArgs e)
        {
            PageFrame.Navigate(new MainPage(this, _displayPreferencesViewModel));
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        /// <summary>
        /// Closes the modal.
        /// </summary>
        protected override void CloseModal()
        {
            if (PageFrame.CanGoBack)
            {
                PageFrame.GoBackWithTransition();
            }
            else
            {
                base.CloseModal();
            }
        }

        /// <summary>
        /// Navigates to view menu.
        /// </summary>
        public void NavigateToViewMenu()
        {
            PageFrame.NavigateWithTransition(new ViewMenuPage(_displayPreferencesViewModel));
        }
    }
}
