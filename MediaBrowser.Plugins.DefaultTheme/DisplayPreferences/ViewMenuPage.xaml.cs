using MediaBrowser.Model.Entities;
using System.Windows;
using MediaBrowser.UI.ViewModels;

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
        }

        ///// <summary>
        ///// Called when [loaded].
        ///// </summary>
        //protected override void OnLoaded()
        //{
        //    base.OnLoaded();

        //    UpdateFields();
        //}

        /// <summary>
        /// Handles the Click event of the radioThumbstrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioThumbstrip_Click(object sender, RoutedEventArgs e)
        {
            MainPage.DisplayPreferences.ScrollDirection = ScrollDirection.Horizontal;
            MainPage.DisplayPreferences.ViewType = ViewTypes.Thumbstrip;
            MainPage.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Handles the Click event of the radioPoster control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioPoster_Click(object sender, RoutedEventArgs e)
        {
            MainPage.DisplayPreferences.ViewType = ViewTypes.Poster;
            MainPage.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Handles the Click event of the radioList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioList_Click(object sender, RoutedEventArgs e)
        {
            MainPage.DisplayPreferences.ScrollDirection = ScrollDirection.Vertical;
            MainPage.DisplayPreferences.ViewType = ViewTypes.List;
            MainPage.NotifyDisplayPreferencesChanged();
        }

        /// <summary>
        /// Updates the fields.
        /// </summary>
        private void UpdateFields()
        {
            var displayPreferences = MainPage.DisplayPreferences;

            radioList.IsChecked = displayPreferences.ViewType == ViewTypes.List;
            radioPoster.IsChecked = displayPreferences.ViewType == ViewTypes.Poster;
            radioThumbstrip.IsChecked = displayPreferences.ViewType == ViewTypes.Thumbstrip;
        }
    }
}
