using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferencesMenu
{
    /// <summary>
    /// Interaction logic for ViewMenuPage.xaml
    /// </summary>
    public partial class ViewMenuPage : BasePage
    {
        private readonly DisplayPreferencesViewModel _displayPreferencesViewModel;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewMenuPage" /> class.
        /// </summary>
        public ViewMenuPage(DisplayPreferencesViewModel displayPreferencesViewModel)
        {
            _displayPreferencesViewModel = displayPreferencesViewModel;
            InitializeComponent();

            DataContext = _displayPreferencesViewModel;
            
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
            _displayPreferencesViewModel.PrimaryImageWidth = 600;

            _displayPreferencesViewModel.ViewType = ListViewTypes.Thumbstrip;
        }

        /// <summary>
        /// Handles the Click event of the radioPoster control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioPoster_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.PrimaryImageWidth = 240;

            _displayPreferencesViewModel.ViewType = ListViewTypes.Poster;
        }

        /// <summary>
        /// Handles the Click event of the radioList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioList_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.PrimaryImageWidth = 140;

            _displayPreferencesViewModel.ViewType = ListViewTypes.List;
        }

        /// <summary>
        /// Updates the fields.
        /// </summary>
        private void UpdateFields()
        {
            radioList.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.List;
            radioPoster.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.Poster;
            radioThumbstrip.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.Thumbstrip;

            if (!radioList.IsChecked.Value && !radioPoster.IsChecked.Value && !radioThumbstrip.IsChecked.Value)
            {
                radioPoster.IsChecked = true;
            }
        }
    }
}
