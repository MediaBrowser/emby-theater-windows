using MediaBrowser.Theater.Presentation.Controls;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Interaction logic for DisplayPreferencesMenu.xaml
    /// </summary>
    public partial class DisplayPreferencesMenu : BaseModalWindow
    {
        public DisplayPreferencesMenu() 
            : base()
        {
            InitializeComponent();

            btnClose.Click += btnClose_Click;
            Loaded += DisplayPreferencesMenu_Loaded;
        }

        public IHasDisplayPreferences DisplayPreferencesContainer { get; set; }

        void DisplayPreferencesMenu_Loaded(object sender, RoutedEventArgs e)
        {
            PageFrame.Navigate(new MainPage { DisplayPreferencesWindow = this });
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
            PageFrame.NavigateWithTransition(new ViewMenuPage { DisplayPreferencesWindow = this });
        }

        /// <summary>
        /// Navigates to sort menu.
        /// </summary>
        public void NavigateToSortMenu()
        {
            PageFrame.NavigateWithTransition(new SortMenuPage { DisplayPreferencesWindow = this });
        }
    }
}
