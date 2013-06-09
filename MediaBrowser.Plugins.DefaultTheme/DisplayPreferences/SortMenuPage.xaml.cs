using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Interaction logic for SortMenuPage.xaml
    /// </summary>
    public partial class SortMenuPage : BaseDisplayPreferencesPage
    {
        private readonly Dictionary<string, string> _sortOptions = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SortMenuPage" /> class.
        /// </summary>
        public SortMenuPage()
        {
            InitializeComponent();

            chkRemember.Click += chkRemember_Click;
            Loaded += SortMenuPage_Loaded;

            _sortOptions["Name"] = ItemSortBy.SortName;
            _sortOptions["CommunityRating"] = ItemSortBy.CommunityRating;
            _sortOptions["Date Added"] = ItemSortBy.DateCreated;
            _sortOptions["Runtime"] = ItemSortBy.Runtime;
            _sortOptions["Year"] = ItemSortBy.ProductionYear;
        }

        void SortMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            AddFields();
        }

        /// <summary>
        /// Handles the Click event of the chkRemember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void chkRemember_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.RememberSorting = true;
        }

        /// <summary>
        /// Adds the fields.
        /// </summary>
        private void AddFields()
        {
            chkRemember.IsChecked = DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.RememberSorting;

            var index = 0;

            var currentValue = DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.SortBy ?? string.Empty;

            foreach (var option in _sortOptions.Keys)
            {
                var optionValue = _sortOptions[option];

                var radio = new ExtendedRadioButton { GroupName = "Options" };

                radio.SetResourceReference(StyleProperty, "ViewMenuRadioButton");

                var textblock = new TextBlock { Text = option };

                textblock.SetResourceReference(StyleProperty, "TextBlockStyle");

                radio.Content = textblock;

                if (string.IsNullOrEmpty(DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.SortBy))
                {
                    radio.IsChecked = index == 0;
                }
                else
                {
                    radio.IsChecked = currentValue.Equals(optionValue, StringComparison.OrdinalIgnoreCase);
                }

                radio.Tag = optionValue;
                radio.Click += radio_Click;

                pnlOptions.Children.Add(radio);

                index++;
            }
        }

        /// <summary>
        /// Handles the Click event of the radio control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radio_Click(object sender, RoutedEventArgs e)
        {
            DisplayPreferencesWindow.DisplayPreferencesContainer.DisplayPreferences.SortBy =
                (sender as RadioButton).Tag.ToString();

            DisplayPreferencesWindow.DisplayPreferencesContainer.NotifyDisplayPreferencesChanged();
        }
    }
}
