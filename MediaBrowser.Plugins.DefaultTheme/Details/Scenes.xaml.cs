using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for Scenes.xaml
    /// </summary>
    public partial class Scenes : UserControl
    {
        private readonly BaseItemDto _item;

        private readonly IApiClient _apiClient;

        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;

        public Scenes(BaseItemDto item, IApiClient apiClient, IImageManager imageManager)
        {
            _item = item;
            _apiClient = apiClient;
            _imageManager = imageManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var items = new RangeObservableCollection<ChapterInfoDtoViewModel>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_item.Chapters.Select(i => new ChapterInfoDtoViewModel(_apiClient, _imageManager)
            {
                Item = _item,
                Chapter = i
            }));
        }
    }
}
