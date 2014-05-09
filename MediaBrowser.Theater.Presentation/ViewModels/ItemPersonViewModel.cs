using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class ItemPersonViewModel : BaseViewModel, IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        private BaseItemPerson _person;
        public BaseItemPerson Person
        {
            get { return _person; }

            set
            {
                var changed = _person != value;

                _person = value;

                if (changed)
                {
                    OnPropertyChanged("Person");
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Role");
                    OnPropertyChanged("PersonType");
                }
            }
        }

        public string Role
        {
            get { return _person == null ? null : _person.Role; }
        }

        public string PersonType
        {
            get
            {
                if (_person == null)
                {
                    return null;
                }

                if (string.Equals(_person.Type, Model.Entities.PersonType.GuestStar, StringComparison.OrdinalIgnoreCase))
                {
                    return "Guest Star";
                }

                return _person.Type;
            }
        }

        public string Name
        {
            get { return _person == null ? null : _person.Name; }
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

        private int? _imageHeight;
        public int? ImageHeight
        {
            get { return _imageHeight; }

            set
            {
                var changed = _imageHeight != value;
                _imageHeight = value;

                if (changed)
                {
                    OnPropertyChanged("ImageHeight");
                }
            }
        }

        private CancellationTokenSource _imageCancellationTokenSource;

        private BitmapImage _image;

        public ItemPersonViewModel(BaseItemPerson person, IApiClient apiClient, IImageManager imageManager)
        {
            _person = person;
            _apiClient = apiClient;
            _imageManager = imageManager;
        }

        public BitmapImage Image
        {
            get
            {
                var img = _image;

                var tokenSource = _imageCancellationTokenSource;

                if (img == null && (tokenSource == null || tokenSource.IsCancellationRequested))
                {
                    DownloadImage();
                }

                return _image;
            }

            private set
            {
                var changed = !Equals(_image, value);

                _image = value;

                if (value == null)
                {
                    var tokenSource = _imageCancellationTokenSource;

                    if (tokenSource != null)
                    {
                        tokenSource.Cancel();
                    }
                }

                if (changed)
                {
                    OnPropertyChanged("Image");
                }
            }
        }

        public async void DownloadImage()
        {
            _imageCancellationTokenSource = new CancellationTokenSource();

            if (Person.PrimaryImageTag != null)
            {
                try
                {
                    var options = new ImageOptions
                    {
                        Height = ImageHeight,
                        Width = ImageWidth,
                        ImageType = ImageType.Primary,
                        Tag = Person.PrimaryImageTag
                    };

                    Image = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetPersonImageUrl(Person, options), _imageCancellationTokenSource.Token);

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
    }
}
