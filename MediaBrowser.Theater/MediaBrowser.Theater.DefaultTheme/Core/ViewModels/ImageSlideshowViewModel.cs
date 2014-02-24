using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ImageSlideshowViewModel
        : BaseViewModel
    {
        private readonly FrameworkElement _defaultImage = new FrameworkElement();
        private readonly Dispatcher _dispatcher;
        private readonly IImageManager _imageManager;
        private readonly RangeObservableCollection<string> _images = new RangeObservableCollection<string>();

        private readonly object _rotationTimerLock = new object();
        private FrameworkElement _currentImage;

        private int _currentIndex = -1;
        private CancellationTokenSource _imageDownloadCancellationTokenSource;
        private Timer _rotationTimer;

        public ImageSlideshowViewModel(IImageManager imageManager, IEnumerable<string> initialImageUrls)
        {
            ImageStretch = Stretch.Uniform;

            _images.AddRange(initialImageUrls);

            _dispatcher = Dispatcher.CurrentDispatcher;
            _imageManager = imageManager;

            StartRotating();
        }

        public Stretch ImageStretch { get; set; }

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                bool changed = _currentIndex != value;

                _currentIndex = value;

                if (changed) {
                    OnPropertyChanged();
                    OnPropertyChanged("CurrentImageUrl");
                    OnIndexChanged();
                }
            }
        }

        public string CurrentImageUrl
        {
            get { return _currentIndex == -1 ? null : _images[_currentIndex]; }
        }

        public RangeObservableCollection<string> Images
        {
            get { return _images; }
        }

        public FrameworkElement CurrentImage
        {
            get { return _currentImage; }
            private set
            {
                bool changed = !Equals(_currentImage, value);

                _currentImage = value;

                if (changed) {
                    OnPropertyChanged();
                    OnPropertyChanged("HasImage");
                }
            }
        }

        public bool HasImage
        {
            get { return _currentImage != null && _currentImage != _defaultImage; }
        }
        
        public void Dispose()
        {
            DisposeRotationTimer();
        }

        public void StartRotating(int invervalMs = 6000)
        {
            //Only set index if we have images
            if (_images.Count > 0 && CurrentIndex == -1) {
                CurrentIndex = 0;
            }

            // We only need the timer if there's more than one image
            if (_images.Count > 1) {
                lock (_rotationTimerLock) {
                    if (_rotationTimer == null) {
                        _rotationTimer = new Timer(OnRotationTimerCallback, null, invervalMs, invervalMs);
                    } else {
                        _rotationTimer.Change(invervalMs, invervalMs);
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
            int numImages = _images.Count;

            int index = CurrentIndex + 1;

            if (numImages == 0) {
                index = -1;
            } else if (index >= numImages) {
                index = 0;
            }

            CurrentIndex = index;
        }

        private async void OnIndexChanged()
        {
            int index = CurrentIndex;

            if (index == -1) {
                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (CurrentImage != null) {
                    CurrentImage = _defaultImage;
                }
                return;
            }

            CancellationTokenSource currentSource = _imageDownloadCancellationTokenSource;

            if (currentSource != null) {
                currentSource.Cancel();
                currentSource.Dispose();
            }

            currentSource = _imageDownloadCancellationTokenSource = new CancellationTokenSource();

            try {
                CancellationToken token = currentSource.Token;

                string image = _images[index];
                Image img = await _imageManager.GetRemoteImageAsync(image, token);

                token.ThrowIfCancellationRequested();

                img.Stretch = ImageStretch;

                CurrentImage = img;
            }
            catch (OperationCanceledException) { }
            catch {
                if (index == 0) {
                    CurrentImage = _defaultImage;
                }
            }
        }

        private void DisposeRotationTimer()
        {
            lock (_rotationTimerLock) {
                if (_rotationTimer != null) {
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
}