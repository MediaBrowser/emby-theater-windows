using System.Linq;
using System.Windows.Controls;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Plugins.DefaultTheme.DisplayPreferences;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.UI;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.Pages;
using System;
using System.Windows;
using MediaBrowser.UI.ViewModels;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for ListPage.xaml
    /// </summary>
    public partial class ListPage : BaseListPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListPage" /> class.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        public ListPage(string itemId)
            : base(itemId)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Subclasses must provide the list box that holds the items
        /// </summary>
        /// <value>The items list.</value>
        protected override ExtendedListBox ItemsList
        {
            get
            {
                return lstItems;
            }
        }

        protected override ImageType PreferredImageType
        {
            get
            {
                if (DisplayPreferences != null)
                {
                    if (DisplayPreferences.ViewType == ViewTypes.Thumbstrip)
                    {
                        return ImageType.Thumb;
                    }
                }
                return base.PreferredImageType;
            }
        }

        /// <summary>
        /// Called when [loaded].
        /// </summary>
        protected override async void OnLoaded()
        {
            base.OnLoaded();

            if (Folder != null)
            {
                ShowViewButton();

                await AppResources.Instance.SetPageTitle(Folder);
            }
            else
            {
                HideViewButton();
            }
        }

        /// <summary>
        /// Called when [unloaded].
        /// </summary>
        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            HideViewButton();
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name.Equals("CurrentItemIndex", StringComparison.OrdinalIgnoreCase))
            {
                UpdateCurrentItemIndex();
            }
        }

        /// <summary>
        /// Updates the index of the current item.
        /// </summary>
        private void UpdateCurrentItemIndex()
        {
            var index = CurrentItemIndex;

            currentItemIndex.Visibility = index == -1 ? Visibility.Collapsed : Visibility.Visible;
            currentItemIndex.Text = (CurrentItemIndex + 1).ToString();

            currentItemIndexDivider.Visibility = index == -1 ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Gets called anytime the Folder gets refreshed
        /// </summary>
        protected override async void OnFolderChanged()
        {
            base.OnFolderChanged();

            var pageTitleTask = AppResources.Instance.SetPageTitle(Folder);

            ShowViewButton();

            if (Folder.IsType("Season"))
            {
                TxtName.Visibility = Visibility.Visible;
                TxtName.Text = Folder.Name;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
            }

            await pageTitleTask;
        }

        /// <summary>
        /// Shows the view button.
        /// </summary>
        private void ShowViewButton()
        {
            var viewButton = AppResources.Instance.ViewButton;
            viewButton.Visibility = Visibility.Visible;
            viewButton.Click -= ViewButton_Click;
            viewButton.Click += ViewButton_Click;
        }

        /// <summary>
        /// Hides the view button.
        /// </summary>
        private void HideViewButton()
        {
            var viewButton = AppResources.Instance.ViewButton;
            viewButton.Visibility = Visibility.Collapsed;
            viewButton.Click -= ViewButton_Click;
        }

        /// <summary>
        /// Handles the Click event of the ViewButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = new DisplayPreferencesMenu
            {
                FolderId = Folder.Id,
                MainPage = this
            };

            menu.ShowModal(this.GetWindow());

            try
            {
                await App.Instance.ApiClient.UpdateDisplayPreferencesAsync(App.Instance.CurrentUser.Id, Folder.Id, DisplayPreferences);
            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Handles current item selection changes
        /// </summary>
        protected override async void OnCurrentItemChanged()
        {
            base.OnCurrentItemChanged();

            var item = CurrentItem;

            if (item == null)
            {
                ItemInfoFooter.Visibility = Visibility.Hidden;
            }
            else
            {
                ItemInfoFooter.Visibility = Visibility.Visible;
                ItemInfoFooter.Item = item;
            }

            if (item != null && item.HasLogo)
            {
                ImgLogo.Source =
                    await App.Instance.GetRemoteBitmapAsync(App.Instance.ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        MaxHeight = 100,
                        ImageType = ImageType.Logo
                    }));

                ImgLogo.Visibility = Visibility.Visible;
            }
            else
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
            }
        }
    }
}
