using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for DetailPage.xaml
    /// </summary>
    public partial class DetailPage : BasePage, ISupportsItemThemeMedia, ISupportsItemBackdrops, IItemPage
    {
        private readonly ItemViewModel _itemViewModel;

        public DetailPage(ItemViewModel itemViewModel)
        {
            _itemViewModel = itemViewModel;
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
            }
        }

        public string ThemeMediaItemId
        {
            get { return _itemViewModel.Item.Id; }
        }

        void PanoramaDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            DefaultTheme.Current.PageContentDataContext.SetPageTitle(_itemViewModel.Item);
        }

        private void SetTitle(BaseItemDto item)
        {
            if (item.Taglines.Count > 0)
            {
                TxtName.Text = item.Taglines[0];
                TxtName.Visibility = Visibility.Visible;
            }
            else if (item.IsType("episode"))
            {
                TxtName.Text = GetEpisodeTitle(item);
                TxtName.Visibility = Visibility.Visible;
            }
            else if (item.IsType("audio"))
            {
                TxtName.Text = GetSongTitle(item);
                TxtName.Visibility = Visibility.Visible;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
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
