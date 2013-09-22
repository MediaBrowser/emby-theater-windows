using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferencesMenu
{
    /// <summary>
    /// Interaction logic for SortMenuPage.xaml
    /// </summary>
    public partial class SortMenuPage : BasePage
    {
        private readonly Dictionary<string, string> _sortOptions = new Dictionary<string, string>();

        private readonly DisplayPreferencesViewModel _displayPreferencesViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortMenuPage" /> class.
        /// </summary>
        public SortMenuPage(DisplayPreferencesViewModel displayPreferencesViewModel, Dictionary<string,string> sortOptions)
        {
            _displayPreferencesViewModel = displayPreferencesViewModel;
            InitializeComponent();

            DataContext = _displayPreferencesViewModel;

            Loaded += SortMenuPage_Loaded;

            _sortOptions = sortOptions;

            RadioAscending.Click += RadioAscending_Click;
            RadioDescending.Click += RadioDescending_Click;
        }

        void RadioDescending_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.SortOrder = SortOrder.Descending;
        }

        void RadioAscending_Click(object sender, RoutedEventArgs e)
        {
            _displayPreferencesViewModel.SortOrder = SortOrder.Ascending;
        }

        void SortMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            AddFields();
        }

        /// <summary>
        /// Adds the fields.
        /// </summary>
        private void AddFields()
        {
            ChkRemember.IsChecked = _displayPreferencesViewModel.RememberSorting;

            var index = 0;

            var currentValue = _displayPreferencesViewModel.SortBy ?? string.Empty;

            foreach (var option in _sortOptions.Keys)
            {
                var optionValue = _sortOptions[option];

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
                radio.Click += radio_Click;

                PnlOptions.Children.Add(radio);

                index++;
            }

            RadioAscending.IsChecked = _displayPreferencesViewModel.DisplayPreferences.SortOrder ==
                                       SortOrder.Ascending;

            RadioDescending.IsChecked = _displayPreferencesViewModel.DisplayPreferences.SortOrder ==
                               SortOrder.Descending;
        }

        /// <summary>
        /// Handles the Click event of the radio control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void radio_Click(object sender, RoutedEventArgs e)
        {
            var radio = (RadioButton)sender;

            _displayPreferencesViewModel.SortBy = radio.Tag.ToString();
        }
    }
}
