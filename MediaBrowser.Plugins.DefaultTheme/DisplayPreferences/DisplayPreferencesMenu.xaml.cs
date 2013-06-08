using MediaBrowser.UI.Controls;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Interaction logic for DisplayPreferencesMenu.xaml
    /// </summary>
    public partial class DisplayPreferencesMenu : BaseModalWindow
    {
        public IHasDisplayPreferences DisplayPreferencesContainer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayPreferencesMenu" /> class.
        /// </summary>
        public DisplayPreferencesMenu()
        {
            InitializeComponent();

            btnClose.Click += btnClose_Click;
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
        /// Called when [loaded].
        /// </summary>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            PageFrame.Navigate(new MainPage { DisplayPreferencesWindow = this });
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
