using System.Windows.Controls;
using System.Windows.Media.Animation;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Plugins.DefaultTheme.Models;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Plugins.DefaultTheme.UserProfileMenu
{
    /// <summary>
    /// Interaction logic for UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow : BaseModalWindow
    {
        private readonly ISessionManager _session;
        private readonly DisplayPreferencesViewModel _displayPreferencesViewModel;
        private readonly ListPageConfig _options;

        private bool _isViewPreferencesOpen;
        private bool _isSortPreferencesOpen;
        private string _previousFocus;

        public UserProfileWindow(DefaultThemePageMasterCommandsViewModel masterCommands, ISessionManager session, IPresentationManager presentationManager, IImageManager imageManager, IApiClient apiClient, DisplayPreferences displayPreferences, ListPageConfig options)
        {
            _session = session;
            _options = options;
            _displayPreferencesViewModel = new DisplayPreferencesViewModel(displayPreferences, presentationManager);
            _previousFocus = "";

            InitializeComponent();

            Loaded += UserProfileWindow_Loaded;
            Unloaded += UserProfileWindow_Unloaded;
            masterCommands.PageNavigated += masterCommands_PageNavigated;
            BtnClose.Click += BtnClose_Click;

            //Display preferences
            RadioList.Click += radioList_Click;
            RadioPoster.Click += radioPoster_Click;
            RadioThumbstrip.Click += radioThumbstrip_Click;
            RadioPosterStrip.Click += radioPosterStrip_Click;

            //Sort preferences
            RadioSortAscending.Click += RadioSortAscending_Click;
            RadioSortDescending.Click += RadioSortDescending_Click;

            //Focus tracking
            BtnClose.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            HomeButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            DisplayPreferencesButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            SortButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            SettingsButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            LogoutButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            ShutdownAppButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            SleepButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            RestartButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            ShutdownButton.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;

            RadioList.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            RadioPosterStrip.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            RadioPoster.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            RadioThumbstrip.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;

            RadioSortAscending.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            RadioSortDescending.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;
            CheckBoxSortRemember.IsKeyboardFocusedChanged += Button_IsKeyboardFocusedChanged;

            ContentGrid.DataContext = new DefaultThemeUserDtoViewModel(masterCommands, apiClient, imageManager, session)
            {
                User = session.CurrentUser,
                ImageHeight = 54
            };

            MainGrid.DataContext = this;
            CheckBoxSortRemember.DataContext = _displayPreferencesViewModel;

            if (displayPreferences != null)
            {
                //Always set to false to begin with in case the user is just doing a quick sort and doesn't want it saved.
                _displayPreferencesViewModel.RememberSorting = false;
            }
        }

        protected override void CloseModal()
        {
            var closeModalStoryboard = (Storyboard) FindResource("ClosingModalStoryboard");
            closeModalStoryboard.Completed += closeModalStoryboard_Completed;
            closeModalStoryboard.Begin();
        }

        /// <summary>
        /// Updates the fields.
        /// </summary>
        private void UpdateFields()
        {
            RadioList.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.List;
            RadioPoster.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.Poster;
            RadioThumbstrip.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.Thumbstrip;
            RadioPosterStrip.IsChecked = _displayPreferencesViewModel.ViewType == ListViewTypes.PosterStrip;

            if (!RadioList.IsChecked.Value && !RadioPoster.IsChecked.Value && !RadioThumbstrip.IsChecked.Value && !RadioPosterStrip.IsChecked.Value)
            {
                RadioPoster.IsChecked = true;
            }
        }

        private void AddFields()
        {
            var index = 0;

            var currentValue = _displayPreferencesViewModel.SortBy ?? string.Empty;

            foreach (var option in _options.SortOptions.Keys)
            {
                var optionValue = _options.SortOptions[option];

                var radio = new RadioButton { GroupName = "Options" };

                radio.Margin = new Thickness(0, 25, 0, 0);

                var textblock = new TextBlock { Text = option };

                textblock.SetResourceReference(StyleProperty, "TextBlockStyle");

                radio.Content = textblock;

                if (string.IsNullOrEmpty(_displayPreferencesViewModel.SortBy))
                {
                    radio.IsChecked = index == 0;
                }
                else
                {
                    radio.IsChecked = currentValue.Equals(optionValue, StringComparison.OrdinalIgnoreCase);
                }

                radio.Tag = optionValue;
                radio.Click += PnlSortOptionsRadio_Click;

                PnlSortOptions.Children.Add(radio);

                index++;
            }

            RadioSortAscending.IsChecked = _displayPreferencesViewModel.DisplayPreferences.SortOrder == SortOrder.Ascending;
            RadioSortDescending.IsChecked = _displayPreferencesViewModel.DisplayPreferences.SortOrder ==  SortOrder.Descending;
        }

        private void OpenViewGrid(PopoutListButton sender)
        {
            _isViewPreferencesOpen = true;
            sender.PopoutModeEnabled = true;
            ViewGrid.SetValue(Grid.VisibilityProperty, Visibility.Visible);
            var sb = FindResource("OpeningViewGridStoryboard") as Storyboard;
            if (sb != null) sb.Begin();
        }

        private void CloseViewGrid(PopoutListButton sender)
        {
            _isViewPreferencesOpen = false;
            sender.PopoutModeEnabled = false;
            ViewGrid.SetValue(Grid.VisibilityProperty, Visibility.Collapsed);
            var sb = FindResource("ClosingViewGridStoryboard") as Storyboard;
            if (sb != null) sb.Begin();
        }

        private void OpenSortGrid(PopoutListButton sender)
        {
            _isSortPreferencesOpen = true;
            sender.PopoutModeEnabled = true;
            SortGrid.SetValue(Grid.VisibilityProperty, Visibility.Visible);
            var sb = FindResource("OpeningSortGridStoryboard") as Storyboard;
            if (sb != null) sb.Begin();
        }

        private void CloseSortGrid(PopoutListButton sender)
        {
            _isSortPreferencesOpen = false;
            sender.PopoutModeEnabled = false;
            SortGrid.SetValue(Grid.VisibilityProperty, Visibility.Collapsed);
            var sb = FindResource("ClosingSortGridStoryboard") as Storyboard;
            if (sb != null) sb.Begin();
        }

        private void RectifyFocus()
        {
            if (_isViewPreferencesOpen && _previousFocus != "DisplayPreferencesButton")
            {
                DisplayPreferencesButton.Focus();
            }
            else if (_isSortPreferencesOpen && _previousFocus != "SortButton")
            {
                SortButton.Focus();
            }
            else
            {
                CloseViewGrid(DisplayPreferencesButton);
                CloseSortGrid(SortButton);
            }
        }

        void closeModalStoryboard_Completed(object sender, EventArgs e)
        {
            if (_displayPreferencesViewModel.DisplayPreferences != null && (bool)CheckBoxSortRemember.IsChecked)
            {
                _displayPreferencesViewModel.Save();
            }

            base.CloseModal();
        }

        void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        void UserProfileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_displayPreferencesViewModel.DisplayPreferences != null)
            {
                AddFields();
                UpdateFields();
            }

            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            _session.UserLoggedOut += _session_UserLoggedOut;
        }

        void UserProfileWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _session.UserLoggedOut -= _session_UserLoggedOut;
        }

        void _session_UserLoggedOut(object sender, EventArgs e)
        {
            CloseModal();
        }

        void masterCommands_PageNavigated(object sender, EventArgs e)
        {
            CloseModal();
        }
        void radioPosterStrip_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.PrimaryImageWidth = _options.PosterStripImageWidth;

            _displayPreferencesViewModel.ViewType = ListViewTypes.PosterStrip;
        }

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

        void RadioSortDescending_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.SortOrder = SortOrder.Descending;
        }

        void RadioSortAscending_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.SortOrder = SortOrder.Ascending;
        }

        void PnlSortOptionsRadio_Click(object sender, RoutedEventArgs e)
        {
            var radio = (RadioButton)sender;

            _displayPreferencesViewModel.SortBy = radio.Tag.ToString();
        }

        // Make sure focusing is restricted when pop out menu is activated and call the correct animations
        private void Button_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var listItem = sender as FrameworkElement;
            var name = listItem.Name;

            if ((bool) e.NewValue)
            {
                if (name == "DisplayPreferencesButton")
                {
                    if (_isSortPreferencesOpen && _previousFocus != "SortButton")
                    {
                        SortButton.Focus();
                    }
                    else
                    {
                        if (!_isViewPreferencesOpen)
                        {
                            CloseSortGrid(SortButton);
                            OpenViewGrid(DisplayPreferencesButton);
                        }
                    }
                }
                else if (name == "SortButton")
                {
                    if (_isViewPreferencesOpen && _previousFocus != "DisplayPreferencesButton")
                    {
                        DisplayPreferencesButton.Focus();
                    }
                    else
                    {
                        if (!_isSortPreferencesOpen)
                        {
                            CloseViewGrid(DisplayPreferencesButton);
                            OpenSortGrid(SortButton);
                        }
                    }
                }
                else if (name == "BtnClose")
                {
                    RectifyFocus();
                }
                else if (name == "HomeButton")
                {
                    RectifyFocus();
                }
                else if (name == "LogoutButton")
                {
                    RectifyFocus();
                }
                else if (name == "SettingsButton")
                {
                    RectifyFocus();
                }
                else if (Name == "ShutdownAppButton")
                {
                    RectifyFocus();
                }
                else if (name == "SleepButton")
                {
                    RectifyFocus();
                }
                else if (name == "RestartButton")
                {
                    RectifyFocus();
                }
                else if (name == "ShutdownButton")
                {
                    RectifyFocus();
                }
            }
            else if (!(bool) e.NewValue)
            {
                _previousFocus = name;
            }
        }
    }
}
