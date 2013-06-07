using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Interaction logic for ItemChapters.xaml
    /// </summary>
    public partial class ItemChapters : BaseDetailsControl
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemChapters" /> class.
        /// </summary>
        public ItemChapters(IApiClient apiClient, IImageManager imageManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            InitializeComponent();

            lstItems.ItemInvoked += lstItems_ItemInvoked;
        }

        /// <summary>
        /// LSTs the items_ item invoked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void lstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var chapterViewModel = (ChapterInfoDtoViewModel) e.Argument;

            //UIKernel.Instance.PlaybackManager.Play(new PlayOptions
            //{
            //    Items = new List<BaseItemDto> { Item },
            //    StartPositionTicks = chapterViewModel.Chapter.StartPositionTicks
            //});
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected override void OnItemChanged()
        {
            const double height = 297;

            var width = ChapterInfoDtoViewModel.GetChapterImageWidth(Item, height, 528);

            var chapters = Item.Chapters ?? new List<ChapterInfoDto> { };

            lstItems.ItemsSource = new ObservableCollection<ChapterInfoDtoViewModel>(chapters.Select(i => new ChapterInfoDtoViewModel(_apiClient, _imageManager)
            {
                Item = Item,
                Chapter = i,
                ImageWidth = width,
                ImageHeight = height,
                ImageDownloadOptions = new ImageOptions
                {
                    MaxHeight = 400
                }
            }));
        }
    }
}
