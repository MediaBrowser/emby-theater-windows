using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Interfaces.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class ImageViewerViewModel : BaseViewModel, IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private readonly IImageManager _imageManager;

        private readonly object _rotationTimerLock = new object();
        private Timer _rotationTimer;

        private int _currentIndex = -1;
        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                var changed = _currentIndex != value;

                _currentIndex = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentIndex");
                    OnIndexChanged();
                }
            }
        }

        private readonly List<ImageViewerImage> _images = new List<ImageViewerImage>();
        public List<ImageViewerImage> Images
        {
            get { return _images; }
        }

        private readonly FrameworkElement _defaultImage = new FrameworkElement();

        private FrameworkElement _currentImage;
        public FrameworkElement CurrentImage
        {
            get
            {
                return _currentImage;
            }
            private set
            {
                var changed = !Equals(_currentImage, value);

                _currentImage = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentImage");
                }
            }
        }

        private string _currentText;
        public string CurrentText
        {
            get
            {
                return _currentText;
            }
            private set
            {
                var changed = !string.Equals(_currentText, value);

                _currentText = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentText");
                }
            }
        }

        public void StartRotating()
        {
            if (CurrentIndex == -1)
            {
                CurrentIndex = 0;
            }

            // We only need the timer if there's more than one image
            if (_images.Count > 1)
            {
                lock (_rotationTimerLock)
                {
                    if (_rotationTimer == null)
                    {
                        _rotationTimer = new Timer(OnRotationTimerCallback, null, 6000, 6000);
                    }
                    else
                    {
                        _rotationTimer.Change(6000, 6000);
                    }
                }
            }
        }

        private void OnRotationTimerCallback(object state)
        {
            _dispatcher.InvokeAsync(MoveToNextImage, DispatcherPriority.Background);
        }

        private void MoveToNextImage()
        {
            var numImages = _images.Count;

            var index = CurrentIndex + 1;

            if (numImages == 0)
            {
                index = -1;
            }
            else if (index >= numImages)
            {
                index = 0;
            }

            CurrentIndex = index;
        }
        
        private CancellationTokenSource _imageDownloadCancellationTokenSource;

        public ImageViewerViewModel(Dispatcher dispatcher, IImageManager imageManager, IEnumerable<ImageViewerImage> initialImages)
        {
            _images.AddRange(initialImages);

            _dispatcher = dispatcher;
            _imageManager = imageManager;
        }

        private async void OnIndexChanged()
        {
            var index = CurrentIndex;

            if (index == -1)
            {
                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (CurrentImage != null)
                {
                    CurrentImage = _defaultImage;
                    CurrentText = string.Empty;
                }
                return;
            }

            if (CurrentImage == null)
            {
                CurrentImage = _defaultImage;
                CurrentText = string.Empty;
            }

            var currentSource = _imageDownloadCancellationTokenSource;

            if (currentSource != null)
            {
                currentSource.Cancel();
                currentSource.Dispose();
            }

            currentSource = _imageDownloadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var token = currentSource.Token;

                var image = _images[index];

                var img = await _imageManager.GetRemoteImageAsync(image.Url, token);

                token.ThrowIfCancellationRequested();

                CurrentImage = img;
                CurrentText = image.Caption;
            }
            catch (OperationCanceledException)
            {

            }
            catch
            {
                if (index == 0)
                {
                    CurrentImage = _defaultImage;
                }
            }
        }

        public void Dispose()
        {
            DisposeRotationTimer();
        }

        private void DisposeRotationTimer()
        {
            lock (_rotationTimerLock)
            {
                if (_rotationTimer != null)
                {
                    _rotationTimer.Dispose();
                    _rotationTimer = null;
                }
            }
        }

        public void StopRotating()
        {
            DisposeRotationTimer();
        }
    }

    public class ImageViewerImage
    {
        public string Url { get; set; }
        public string Caption { get; set; }
    }
}
