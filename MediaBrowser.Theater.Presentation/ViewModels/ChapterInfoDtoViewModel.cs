using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using System;
using System.Linq;
using System.Windows.Media.Imaging;
using MediaBrowser.Theater.Interfaces.Presentation;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class ChapterInfoDtoViewModel
    /// </summary>
    public class ChapterInfoDtoViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the image download options.
        /// </summary>
        /// <value>The image download options.</value>
        public ImageOptions ImageDownloadOptions { get; set; }

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
        /// The _image width
        /// </summary>
        private double _imageWidth;
        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image.</value>
        public double ImageWidth
        {
            get { return _imageWidth; }

            set
            {
                _imageWidth = value;
                OnPropertyChanged("ImageWidth");
            }
        }

        /// <summary>
        /// The _image height
        /// </summary>
        private double _imageHeight;
        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        /// <value>The height of the image.</value>
        public double ImageHeight
        {
            get { return _imageHeight; }

            set
            {
                _imageHeight = value;
                OnPropertyChanged("ImageHeight");
            }
        }

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
                _chapter = value;
                OnPropertyChanged("Chapter");
                OnPropertyChanged("TimeString");
                OnChapterChanged();
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
                _item = value;
                OnPropertyChanged("Item");
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

        /// <summary>
        /// The _image
        /// </summary>
        private BitmapImage _image;

        public ChapterInfoDtoViewModel(IApiClient apiClient, IImageManager imageManager)
        {
            ImageManager = imageManager;
            ApiClient = apiClient;
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>The image.</value>
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private async void OnChapterChanged()
        {
            var options = ImageDownloadOptions ?? new ImageOptions { };

            options.ImageType = ImageType.Chapter;
            options.ImageIndex = Item.Chapters.IndexOf(Chapter);

            try
            {
                Image = await ImageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(Item, options));
            }
            catch (HttpException)
            {
            }
        }

        /// <summary>
        /// Gets the height of the chapter image.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="height">The height.</param>
        /// <param name="defaultWidth">The default width.</param>
        /// <returns>System.Double.</returns>
        public static double GetChapterImageWidth(BaseItemDto item, double height, double defaultWidth)
        {
            var width = defaultWidth;

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

                        width = height * aspectRatio;
                    }
                }
            }

            return width;
        }
    }
}
