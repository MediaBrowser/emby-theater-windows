using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MediaBrowser.Theater.Interfaces.ViewModels;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class RotatingBackdropsViewModel : BaseViewModel, IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly ILogger _logger;

        private readonly object _rotationTimerLock = new object();
        private readonly object _initialSetTimerLock = new object();

        private Timer _backdropSetTimer;
        private Timer _backdropRotationTimer;

        private string[] _currentBackdrops;
        
        public RotatingBackdropsViewModel(IApiClient apiClient, ITheaterConfigurationManager config, IImageManager imageManager, IPlaybackManager playbackManager, ILogger logger)
        {
            _apiClient = apiClient;
            _config = config;
            _imageManager = imageManager;
            _playbackManager = playbackManager;
            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private int _currentBackdropIndex;
        public int CurrentBackdropIndex
        {
            get { return _currentBackdropIndex; }
            set
            {
                _currentBackdropIndex = value;
                OnBackdropIndexChanged();
            }
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

        /// <summary>
        /// Sets the backdrop based on a BaseItemDto
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetBackdrops(BaseItemDto item)
        {
            var urls = _apiClient.GetBackdropImageUrls(item, new ImageOptions
            {
                Width = Convert.ToInt32(SystemParameters.VirtualScreenWidth)
            });

            SetBackdrops(urls);
        }

        /// <summary>
        /// Clears the backdrops.
        /// </summary>
        public void ClearBackdrops()
        {
            SetBackdrops(new string[] { });
        }

        public void SetBackdrops(string[] backdrops)
        {
            if (!_config.Configuration.EnableBackdrops)
            {
                backdrops = new string[] { };
            }

            lock (_initialSetTimerLock)
            {
                _pendingBackdrops = backdrops;

                if (_backdropSetTimer == null)
                {
                    _backdropSetTimer = new Timer(OnPendingBackdropsTimerFired, null, 300, Timeout.Infinite);
                }
                else
                {
                    _backdropSetTimer.Change(300, Timeout.Infinite);
                }
            }
        }

        private string[] _pendingBackdrops;
        private void OnPendingBackdropsTimerFired(object state)
        {
            _dispatcher.InvokeAsync(() => SetBackdropsInternal(_pendingBackdrops), DispatcherPriority.Background);
        }

        /// <summary>
        /// Sets the backdrop based on a list of image files
        /// </summary>
        /// <param name="backdrops">The backdrops.</param>
        public void SetBackdropsInternal(string[] backdrops)
        {
            // Don't reload the same backdrops
            if (_currentBackdrops != null && backdrops.SequenceEqual(_currentBackdrops))
            {
                return;
            }

            var currentSource = _backdropDownloadCancellationTokenSource;

            if (currentSource != null)
            {
                currentSource.Cancel();
                currentSource.Dispose();
                _backdropDownloadCancellationTokenSource = null;
            }
            
            _currentBackdrops = backdrops;

            if (backdrops == null || backdrops.Length == 0)
            {
                CurrentBackdropIndex = -1;
                return;
            }

            CurrentBackdropIndex = 0;

            // We only need the timer if there's more than one backdrop
            if (backdrops.Length > 1)
            {
                lock (_rotationTimerLock)
                {
                    const int rotationPeriodMs = 7000;

                    if (_backdropRotationTimer == null)
                    {
                        _backdropRotationTimer = new Timer(OnBackdropRotationTimerCallback, null, rotationPeriodMs, rotationPeriodMs);
                    }
                    else
                    {
                        _backdropRotationTimer.Change(rotationPeriodMs, rotationPeriodMs);
                    }
                }
            }
        }

        private void OnBackdropRotationTimerCallback(object state)
        {
            _dispatcher.InvokeAsync(MoveToNextBackdrop, DispatcherPriority.Background);
        }

        private void MoveToNextBackdrop()
        {
            var backdrops = _currentBackdrops;

            var index = CurrentBackdropIndex + 1;

            if (backdrops == null || backdrops.Length == 0)
            {
                index = -1;
            }
            else if (index >= backdrops.Length)
            {
                index = 0;
            }

            CurrentBackdropIndex = index;
        }

        private CancellationTokenSource _backdropDownloadCancellationTokenSource;

        /// <summary>
        /// Called when [backdrop index changed].
        /// </summary>
        private async void OnBackdropIndexChanged()
        {
            var index = CurrentBackdropIndex;

            if (index == -1)
            {
                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (CurrentImage != null)
                {
                    CurrentImage = _defaultImage;
                }
                return;
            }

            if (index > 0)
            {
                // Don't display backdrops during video playback
                if (_playbackManager.MediaPlayers.Any(p =>
                {
                    if (p.PlayState != PlayState.Idle)
                    {
                        var media = p.CurrentMedia;
                        return media != null && (media.IsVideo || media.IsGame);
                    }
                    return false;
                }))
                {
                    return;
                }
            }

            if (CurrentImage == null)
            {
                CurrentImage = _defaultImage;
            }

            _logger.Info("Setting backdtop to {0}", _currentBackdrops[index]);

            var currentSource = _backdropDownloadCancellationTokenSource;

            if (currentSource != null)
            {
                currentSource.Cancel();
                currentSource.Dispose();
            }

            currentSource = _backdropDownloadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var token = currentSource.Token;

                var img = await _imageManager.GetRemoteImageAsync(_currentBackdrops[index], token);

                token.ThrowIfCancellationRequested();

                img.SetResourceReference(FrameworkElement.StyleProperty, "BackdropImage");

                CurrentImage = img;
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
            DisposeInitialSetTimer();
        }

        private void DisposeRotationTimer()
        {
            lock (_rotationTimerLock)
            {
                if (_backdropRotationTimer != null)
                {
                    _backdropRotationTimer.Dispose();
                    _backdropRotationTimer = null;
                }
            }
        }

        private void DisposeInitialSetTimer()
        {
            lock (_initialSetTimerLock)
            {
                if (_backdropSetTimer != null)
                {
                    _backdropSetTimer.Dispose();
                    _backdropSetTimer = null;
                }
            }
        }

    }
}
