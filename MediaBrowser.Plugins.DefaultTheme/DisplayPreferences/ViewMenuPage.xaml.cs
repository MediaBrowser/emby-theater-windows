using MediaBrowser.Model.Entities;
using MediaBrowser.UI.ViewModels;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Interaction logic for ViewMenuPage.xaml
    /// </summary>
    public partial class ViewMenuPage : BaseDisplayPreferencesPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewMenuPage" /> class.
        /// </summary>
        public ViewMenuPage()
        {
            InitializeComponent();

            radioList.Click += radioList_Click;
            radioPoster.Click += radioPoster_Click;
            radioThumbstrip.Click += radioThumbstrip_Click;
            Loaded += ViewMenuPage_Loaded;
        }

        void ViewMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFields();
        }

        /// <summary>
        /// Handles the Click event of the radioThumbstrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioThumbstrip_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.ScrollDirection = ScrollDirection.Horizontal;
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.ViewType = ViewTypes.Thumbstrip;
            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Handles the Click event of the radioPoster control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioPoster_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.ViewType = ViewTypes.Poster;
            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Handles the Click event of the radioList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioList_Click(object sender, RoutedEventArgs e)
        {
            var displayPreferences = DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences;

            displayPreferences.ScrollDirection = ScrollDirection.Vertical;
            displayPreferences.ViewType = ViewTypes.List;
            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Updates the fields.
        /// </summary>
        private void UpdateFields()
        {
            var displayPreferences = DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences;

            radioList.IsChecked = displayPreferences.ViewType == ViewTypes.List;
            radioPoster.IsChecked = displayPreferences.ViewType == ViewTypes.Poster;
            radioThumbstrip.IsChecked = displayPreferences.ViewType == ViewTypes.Thumbstrip;
        }
    }
}
