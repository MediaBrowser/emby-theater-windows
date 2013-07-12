using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
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

        private readonly IPlaybackManager _playbackManager;
        
        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;

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
            
            var items = new RangeObservableCollection<ChapterInfoDtoViewModel>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_item.Chapters.Take(9).Select(i => new ChapterInfoDtoViewModel(_apiClient, _imageManager)
            {
                Item = _item,
                Chapter = i
            }));
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var item = (ChapterInfoDtoViewModel)e.Argument;

            await _playbackManager.Play(new PlayOptions(_item)
            {
                StartPositionTicks = item.Chapter.StartPositionTicks
            });
        }
    }
}
