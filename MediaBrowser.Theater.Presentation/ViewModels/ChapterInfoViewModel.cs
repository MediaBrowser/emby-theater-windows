using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class ChapterInfoDtoViewModel
    /// </summary>
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class ChapterInfoViewModel : BaseViewModel, IDisposable
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        private readonly IImageManager _imageManager;
        private readonly IPlaybackManager _playbackManager;

        public ChapterInfoViewModel(IApiClient apiClient, IImageManager imageManager, IPlaybackManager playbackManager)
        {
            _imageManager = imageManager;
            _playbackManager = playbackManager;
            _apiClient = apiClient;
        }

        private CancellationTokenSource _imageCancellationTokenSource = null;

        private BitmapImage _image;
        public BitmapImage Image
        {
            get
            {
                var img = _image;

                if (img == null && _imageCancellationTokenSource == null)
                {
                    DownloadImage();
                }

                return _image;
            }

            private set
            {
                var changed = !Equals(_image, value);

                _image = value;

                if (changed)
                {
                    OnPropertyChanged("Image");
                }
            }
        }

        private bool _isImageLoading = true;
        public bool IsImageLoading
        {
            get { return _isImageLoading; }

            set
            {
                var changed = _isImageLoading != value;
                _isImageLoading = value;

                if (changed)
                {
                    OnPropertyChanged("IsImageLoading");
                }
            }
        }

        private bool _hasImage;
        public bool HasImage
        {
            get { return _hasImage; }

            set
            {
                var changed = _hasImage != value;
                _hasImage = value;

                if (changed)
                {
                    OnPropertyChanged("HasImage");
                }
            }
        }

        private int? _imageWidth;
        public int? ImageWidth
        {
            get { return _imageWidth; }

            set
            {
                var changed = _imageWidth != value;
                _imageWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageWidth");
                }
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

        public async void DownloadImage()
        {
            _imageCancellationTokenSource = new CancellationTokenSource();

            if (Chapter.ImageTag != null)
            {
                try
                {
                    var options = new ImageOptions
                    {
                        Width = ImageWidth,
                        ImageIndex = Item.Chapters.IndexOf(Chapter),
                        ImageType = ImageType.Chapter,
                        Tag = Chapter.ImageTag
                    };

                    Image = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetImageUrl(Item, options),
                                                           _imageCancellationTokenSource.Token);

                    HasImage = true;
                    IsImageLoading = false;
                }
                catch (OperationCanceledException)
                {

                }
                catch
                {
                    // Logged at lower levels
                    HasImage = false;
                    IsImageLoading = false;
                }
                finally
                {
                    DisposeCancellationTokenSource();
                }
            }
            else
            {
                HasImage = false;
                IsImageLoading = false;
            }

        }

        public void Dispose()
        {
            DisposeCancellationTokenSource();
        }

        private void DisposeCancellationTokenSource()
        {
            if (_imageCancellationTokenSource != null)
            {
                _imageCancellationTokenSource.Cancel();
                _imageCancellationTokenSource.Dispose();
                _imageCancellationTokenSource = null;
            }
        }

        public Task Play()
        {
            return _playbackManager.Play(new PlayOptions(_item)
            {
                StartPositionTicks = Chapter.StartPositionTicks
            });
        }
    }
}
