using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows;
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

        private readonly IPlaybackManager _playbackManager;

        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;

        private readonly RangeObservableCollection<ChapterInfoDtoViewModel> _listItems =
            new RangeObservableCollection<ChapterInfoDtoViewModel>();

        public Scenes(BaseItemDto item, IApiClient apiClient, IImageManager imageManager, IPlaybackManager playbackManager)
        {
            _item = item;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _playbackManager = playbackManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_listItems);
            LstItems.ItemsSource = view;

            _listItems.AddRange(_item.Chapters.Select(i => new ChapterInfoDtoViewModel(_apiClient, _imageManager, _playbackManager)
            {
                Item = _item,
                Chapter = i,
                ImageDownloadWidth = 380
            }));

            Unloaded += Scenes_Unloaded;
        }

        void Scenes_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var viewModel in _listItems.ToList())
            {
                viewModel.Dispose();
            }

            _listItems.Clear();
        }

        void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var item = (ChapterInfoDtoViewModel)e.Argument;

            item.Play();
        }
    }
}
