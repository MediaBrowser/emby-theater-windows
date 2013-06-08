using MediaBrowser.Model.Entities;
using MediaBrowser.UI.ViewModels;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : BaseDisplayPreferencesPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            btnScroll.Click += btnScroll_Click;
            btnIncrease.Click += btnIncrease_Click;
            btnDecrease.Click += btnDecrease_Click;
            ViewMenuButton.Click += ViewMenuButton_Click;
            SortMenuButton.Click += SortMenuButton_Click;
            Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the MainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFields();
        }

        /// <summary>
        /// Handles the Click event of the SortMenuButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void SortMenuButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.NavigateToSortMenu();
        }

        /// <summary>
        /// Handles the Click event of the ViewMenuButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void ViewMenuButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.NavigateToViewMenu();
        }

        /// <summary>
        /// Handles the Click event of the btnDecrease control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.DecreaseImageSize();
            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Handles the Click event of the btnIncrease control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.IncreaseImageSize();
            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Handles the Click event of the btnScroll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void btnScroll_Click(object sender, RoutedEventArgs e)
        {
            var displayPreferences = DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences;

            displayPreferences.ScrollDirection = displayPreferences.ScrollDirection == ScrollDirection.Horizontal
                                                     ? ScrollDirection.Vertical
                                                     : ScrollDirection.Horizontal;

            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
           
            UpdateFields();
        }

        /// <summary>
        /// Updates the fields.
        /// </summary>
        private void UpdateFields()
        {
            var displayPreferences = DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences;

            btnScroll.Visibility = displayPreferences.ViewType == ViewTypes.Poster || string.IsNullOrEmpty(displayPreferences.ViewType)
                                       ? Visibility.Visible
                                       : Visibility.Collapsed;

            txtScrollDirection.Text = displayPreferences.ScrollDirection == ScrollDirection.Horizontal ? "Scroll: Horizontal" : "Scroll: Vertical";
        }
    }
}
