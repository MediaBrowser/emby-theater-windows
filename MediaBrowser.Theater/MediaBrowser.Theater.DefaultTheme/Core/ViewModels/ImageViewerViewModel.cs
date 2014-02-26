using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel, IDisposable
    {
        private readonly FrameworkElement _defaultImage = new FrameworkElement();
        private readonly Dispatcher _dispatcher;
        private readonly IImageManager _imageManager;
        private readonly RangeObservableCollection<ImageViewerImage> _images = new RangeObservableCollection<ImageViewerImage>();

        private readonly object _rotationTimerLock = new object();
        private FrameworkElement _currentImage;

        private int _currentIndex = -1;
        private string _currentText;
        private CancellationTokenSource _imageDownloadCancellationTokenSource;
        private Timer _rotationTimer;

        public ImageViewerViewModel(IImageManager imageManager, IEnumerable<ImageViewerImage> initialImages)
        {
            ImageStretch = Stretch.Uniform;

            _images.AddRange(initialImages);

            _dispatcher = Dispatcher.CurrentDispatcher;
            _imageManager = imageManager;

            CustomCommand = new RelayCommand(o => {
                ImageViewerImage img = _currentIndex == -1 ? null : Images[_currentIndex];

                if (img != null) {
                    CustomCommandAction(img);
                }
            });
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
                    OnIndexChanged();
                }
            }
        }

        public RangeObservableCollection<ImageViewerImage> Images
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

        public string CurrentText
        {
            get { return _currentText; }
            private set
            {
                bool changed = !string.Equals(_currentText, value);

                _currentText = value;

                if (changed) {
                    OnPropertyChanged();
                }
            }
        }

        public Action<ImageViewerImage> CustomCommandAction { get; set; }

        public ICommand CustomCommand { get; private set; }

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
                    CurrentText = string.Empty;
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

                ImageViewerImage image = _images[index];

                Image img = await _imageManager.GetRemoteImageAsync(image.Url, token);

                token.ThrowIfCancellationRequested();

                img.Stretch = ImageStretch;

                CurrentImage = img;

                await Task.Delay(200, token);

                CurrentText = image.Caption;
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

    public class ImageViewerImage : BaseViewModel
    {
        /// <summary>
        ///     The _item
        /// </summary>
        private BaseItemDto _item;

        /// <summary>
        ///     Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                bool changed = _item != value;
                _item = value;

                if (changed) {
                    OnPropertyChanged();
                }
            }
        }

        public string Url { get; set; }
        public string Caption { get; set; }
    }
}