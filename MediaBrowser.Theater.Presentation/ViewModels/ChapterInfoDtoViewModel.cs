using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class ChapterInfoDtoViewModel
    /// </summary>
    public class ChapterInfoDtoViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public IApiClient ApiClient { get; private set; }
        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        public IImageManager ImageManager { get; private set; }

        /// <summary>
        /// The _item
        /// </summary>
        private ChapterInfoDto _chapter;
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public ChapterInfoDto Chapter
        {
            get { return _chapter; }

            set
            {
                var changed = _chapter != value;
                _chapter = value;

                if (changed)
                {
                    OnPropertyChanged("Chapter");
                    OnPropertyChanged("TimeString");
                }
            }
        }


        /// <summary>
        /// The _item
        /// </summary>
        private BaseItemDto _item;
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                var changed = _item != value;
                _item = value;

                if (changed)
                {
                    OnPropertyChanged("Item");
                }
            }
        }

        /// <summary>
        /// Gets the time string.
        /// </summary>
        /// <value>The time string.</value>
        public string TimeString
        {
            get
            {
                var time = TimeSpan.FromTicks(Chapter.StartPositionTicks);

                return time.ToString(time.TotalHours < 1 ? "m':'ss" : "h':'mm':'ss");
            }
        }

        public ChapterInfoDtoViewModel(IApiClient apiClient, IImageManager imageManager)
        {
            ImageManager = imageManager;
            ApiClient = apiClient;
        }

        public Task<BitmapImage> GetImage(ImageOptions options)
        {
            options.ImageIndex = Item.Chapters.IndexOf(Chapter);
            options.ImageType = ImageType.Chapter;
            options.Tag = Chapter.ImageTag;

            return ImageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(Item, options));
        }

        /// <summary>
        /// Gets the height of the chapter image.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="width">The width.</param>
        /// <param name="defaultHeight">The default height.</param>
        /// <returns>System.Double.</returns>
        public static double GetChapterImageHeight(BaseItemDto item, double width, double defaultHeight)
        {
            var height = defaultHeight;

            if (item.MediaStreams != null)
            {
                var videoStream = item.MediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Video);

                if (videoStream != null)
                {
                    double streamHeight = videoStream.Height ?? 0;
                    double streamWidth = videoStream.Width ?? 0;

                    if (streamHeight > 0 && streamWidth > 0)
                    {
                        var aspectRatio = streamWidth / streamHeight;

                        height = width / aspectRatio;
                    }
                }
            }

            return height;
        }
    }
}
