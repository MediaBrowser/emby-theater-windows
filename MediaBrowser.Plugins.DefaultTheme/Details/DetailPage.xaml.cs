using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for DetailPage.xaml
    /// </summary>
    public partial class DetailPage : BasePage, ISupportsItemThemeMedia, ISupportsItemBackdrops, IItemPage
    {
        private readonly ItemViewModel _itemViewModel;
        private readonly IPresentationManager _presentation;

        public DetailPage(ItemViewModel itemViewModel, IPresentationManager presentation)
        {
            _itemViewModel = itemViewModel;
            _presentation = presentation;
            InitializeComponent();

            Loaded += PanoramaDetailPage_Loaded;

            SetTitle(_itemViewModel.Item);

            DataContextChanged += PanoramaDetailPage_DataContextChanged;
        }

        void PanoramaDetailPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.NewValue as DetailPageViewModel;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += viewModel_PropertyChanged;
            }
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "CurrentSection"))
            {
                ScrollViewer.ScrollToLeftEnd();

                var current = MenuList.ItemContainerGenerator.ContainerFromItem(MenuList.SelectedItem) as ListBoxItem;

                if (current != null)
                {
                    current.Focus();
                }
            }
        }

        /// <summary>
        /// If viewing a person, genre, etc. This is the id of the media item that we came from (could be null)
        /// </summary>
        public string ItemByNameMediaItemId { get; set; }

        public string ThemeMediaItemId
        {
            get { return ItemByNameMediaItemId ?? _itemViewModel.Item.Id; }
        }

        void PanoramaDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            var item = _itemViewModel.Item;

            if (item.IsPerson || item.IsArtist || item.IsGenre || item.IsGameGenre || item.IsMusicGenre || item.IsStudio)
            {
                _presentation.SetDefaultPageTitle();
            }
            else
            {
                DefaultTheme.Current.PageContentDataContext.SetPageTitle(item);
            }
        }

        private void SetTitle(BaseItemDto item)
        {
            if (item.Taglines.Count > 0)
            {
                TxtTagline.Text = item.Taglines[0];
                TxtTagline.Visibility = Visibility.Visible;
                TxtName.Visibility = Visibility.Collapsed;
            }
            else if (item.IsType("episode"))
            {
                TxtName.Text = GetEpisodeTitle(item);
                TxtName.Visibility = Visibility.Visible;
                TxtTagline.Visibility = Visibility.Collapsed;
            }
            else if (item.IsType("audio"))
            {
                TxtName.Text = GetSongTitle(item);
                TxtName.Visibility = Visibility.Visible;
                TxtTagline.Visibility = Visibility.Collapsed;
            }
            else if (item.IsPerson || item.IsArtist || item.IsGenre || item.IsGameGenre || item.IsMusicGenre || item.IsStudio)
            {
                TxtName.Text = item.Name;
                TxtName.Visibility = Visibility.Visible;
                TxtTagline.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
                TxtTagline.Visibility = Visibility.Collapsed;
            }
        }

        private static string GetEpisodeTitle(BaseItemDto item)
        {
            var title = item.Name;

            if (item.IndexNumber.HasValue)
            {
                title = "Ep. " + item.IndexNumber.Value + " - " + title;
            }

            if (item.ParentIndexNumber.HasValue)
            {
                title = string.Format("Season {0}, ", item.ParentIndexNumber.Value) + title;
            }

            return title;
        }

        private string GetSongTitle(BaseItemDto item)
        {
            return item.Name;
        }

        public BaseItemDto BackdropItem
        {
            get { return _itemViewModel.Item; }
        }

        public BaseItemDto PageItem
        {
            get { return _itemViewModel.Item; }
        }

        public ViewType ViewType
        {
            get { return ViewType.Folders; }
        }
    }
}
