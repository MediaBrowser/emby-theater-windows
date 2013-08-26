using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class GalleryImageViewModel : BaseViewModel, IDisposable
    {
        private readonly IImageManager _imageManager;

        private string _imageUrl;
        public string ImageUrl
        {
            get { return _imageUrl; }

            set
            {
                var changed = !string.Equals(_imageUrl, value);
                _imageUrl = value;

                if (changed)
                {
                    OnPropertyChanged("ImageUrl");
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

        private CancellationTokenSource _imageCancellationTokenSource = null;

        private BitmapImage _image;

        public GalleryImageViewModel(IImageManager imageManager)
        {
            _imageManager = imageManager;
        }

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

        public async void DownloadImage()
        {
            _imageCancellationTokenSource = new CancellationTokenSource();

            try
            {
                Image = await _imageManager.GetRemoteBitmapAsync(ImageUrl, _imageCancellationTokenSource.Token);

                HasImage = true;
            }
            catch
            {
                HasImage = false;
            }
            finally
            {
                DisposeCancellationTokenSource();
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
