using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class UserDtoViewModel
    /// </summary>
    public class UserDtoViewModel : BaseViewModel, IDisposable
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDtoViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageManager">The image manager.</param>
        public UserDtoViewModel(IApiClient apiClient, IImageManager imageManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
        }

        public string Username
        {
            get { return _item == null ? null : _item.Name; }
        }

        /// <summary>
        /// The _item
        /// </summary>
        private UserDto _item;
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user.</value>
        public UserDto User
        {
            get { return _item; }

            set
            {
                var changed = _item != value;

                _item = value;

                if (changed)
                {
                    OnPropertyChanged("User");
                    OnPropertyChanged("Username");
                }
            }
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

        private int? _imageDownloadWidth;
        public int? ImageDownloadWidth
        {
            get { return _imageDownloadWidth; }

            set
            {
                var changed = _imageDownloadWidth != value;
                _imageDownloadWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageDownloadWidth");
                }
            }
        }

        public async void DownloadImage()
        {
            _imageCancellationTokenSource = new CancellationTokenSource();

            if (User.PrimaryImageTag.HasValue)
            {
                try
                {
                    var options = new ImageOptions
                    {
                        Width = ImageDownloadWidth,
                        ImageType = ImageType.Primary
                    };

                    Image = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetUserImageUrl(User, options), _imageCancellationTokenSource.Token);

                    DisposeCancellationTokenSource();

                    HasImage = true;
                }
                catch
                {
                    // Logged at lower levels
                    HasImage = false;
                }
            }
            else
            {
                HasImage = false;
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

        /// <summary>
        /// Gets the relative time text.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>System.String.</returns>
        private static string GetRelativeTimeText(DateTime date)
        {
            var ts = DateTime.Now - date;

            const int second = 1;
            const int minute = 60 * second;
            const int hour = 60 * minute;
            const int day = 24 * hour;
            const int month = 30 * day;

            var delta = Convert.ToInt32(ts.TotalSeconds);

            if (delta < 0)
            {
                return "not yet";
            }
            if (delta < 1 * minute)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 2 * minute)
            {
                return "a minute ago";
            }
            if (delta < 45 * minute)
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 90 * minute)
            {
                return "an hour ago";
            }
            if (delta < 24 * hour)
            {
                return ts.Hours == 1 ? "an hour ago" : ts.Hours + " hours ago";
            }
            if (delta < 48 * hour)
            {
                return "yesterday";
            }
            if (delta < 30 * day)
            {
                return ts.Days + " days ago";
            }
            if (delta < 12 * month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

    }
}
