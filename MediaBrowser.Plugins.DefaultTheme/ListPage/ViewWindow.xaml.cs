using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.ListPage
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : BaseModalWindow
    {
        private readonly DisplayPreferencesViewModel _displayPreferencesViewModel;

        private ListPageConfig _options;
        
        public ViewWindow(DisplayPreferencesViewModel displayPreferencesViewModel, ListPageConfig options)
        {
            _displayPreferencesViewModel = displayPreferencesViewModel;
            _options = options;
            InitializeComponent();

            DataContext = this;

            btnClose.Click += btnClose_Click;
            radioList.Click += radioList_Click;
            radioPoster.Click += radioPoster_Click;
            radioThumbstrip.Click += radioThumbstrip_Click;

            Loaded += SortMenuPage_Loaded;
        }

        void SortMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFields();
            
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        /// <summary>
        /// Handles the Click event of the radioThumbstrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioThumbstrip_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.PrimaryImageWidth = _options.ThumbImageWidth;

            _displayPreferencesViewModel.ViewType = ListViewTypes.Thumbstrip;
        }

        /// <summary>
        /// Handles the Click event of the radioPoster control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioPoster_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.PrimaryImageWidth = _options.PosterImageWidth;

            _displayPreferencesViewModel.ViewType = ListViewTypes.Poster;
        }

        /// <summary>
        /// Handles the Click event of the radioList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radioList_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.PrimaryImageWidth = _options.ListImageWidth;

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
