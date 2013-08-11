using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using Microsoft.Expression.Media.Effects;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MediaBrowser.UI.Implementations
{
    public class RotatingBackdrops : IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private readonly TransitionControl _backdropContainer;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly ILogger _logger;

        private readonly object _rotationTimerLock = new object();
        private readonly object _initialSetTimerLock = new object();

        private Timer _backdropSetTimer;
        private Timer _backdropRotationTimer;

        /// <summary>
        /// Gets or sets the current backdrops.
        /// </summary>
        /// <value>The current backdrops.</value>
        private string[] _currentBackdrops;

        private int _currentBackdropIndex;

        public RotatingBackdrops(Dispatcher dispatcher, TransitionControl backdropContainer, IImageManager imageManager, IApiClient apiClient, IPlaybackManager playbackManager, ITheaterConfigurationManager config, ILogger logger)
        {
            _dispatcher = dispatcher;
            _backdropContainer = backdropContainer;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _playbackManager = playbackManager;
            _config = config;
            _logger = logger;

            _config.UserConfigurationUpdated += _config_UserConfigurationUpdated;
        }

        void _config_UserConfigurationUpdated(object sender, UserConfigurationUpdatedEventArgs e)
        {
            _dispatcher.InvokeAsync(() => _backdropContainer.TransitionType = GetTransitionEffect(e.Configuration.BackdropTransition));
        }

        private TransitionEffect GetTransitionEffect(string name)
        {
            switch (name.ToLower())
            {
                case "blur":
                    return new RadialBlurTransitionEffect();
                case "circle reveal":
                    return new CircleRevealTransitionEffect();
                case "cloud reveal":
                    return new CloudRevealTransitionEffect();
                case "fade":
                    return new FadeTransitionEffect();
                case "horizontal blinds":
                    return new BlindsTransitionEffect { Orientation = BlindOrientation.Horizontal };
                case "horizontal slide":
                    return new SlideInTransitionEffect { SlideDirection = SlideDirection.RightToLeft };
                case "horizontal wipe":
                    return new WipeTransitionEffect { WipeDirection = WipeDirection.RightToLeft };
                case "ripple":
                    return new RippleTransitionEffect();
                case "smooth swirl":
                    return new SmoothSwirlGridTransitionEffect();
                case "vertical blinds":
                    return new BlindsTransitionEffect { Orientation = BlindOrientation.Vertical };
                case "vertical slide":
                    return new SlideInTransitionEffect { SlideDirection = SlideDirection.TopToBottom };
                case "vertical wipe":
                    return new WipeTransitionEffect { WipeDirection = WipeDirection.TopToBottom };
                case "wave":
                    return new WaveTransitionEffect();
                default:
                    return null;
            }
        }

        public int CurrentBackdropIndex
        {
            get { return _currentBackdropIndex; }
            set
            {
                _currentBackdropIndex = value;
                OnBackdropIndexChanged();
            }
        }

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
                if (_backdropContainer.Content != null)
                {
                    _backdropContainer.Content = new FrameworkElement();
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

            if (_backdropContainer.Content == null)
            {
                _backdropContainer.Content = new FrameworkElement();
            }
            
            _logger.Info("Setting backdtop to {0}", _currentBackdrops[index]);
            
            try
            {
                var bitmap = await _imageManager.GetRemoteBitmapAsync(_currentBackdrops[index]);

                var img = new Image
                {
                    Source = bitmap
                };

                img.SetResourceReference(FrameworkElement.StyleProperty, "BackdropImage");

                _backdropContainer.Content = img;
            }
            catch (HttpException)
            {
                if (index == 0)
                {
                    _backdropContainer.Content = new FrameworkElement();
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
                    _backdropSetTimer = new Timer(OnPendingBackdropsTimerFired, null, 1000, Timeout.Infinite);
                }
                else
                {
                    _backdropSetTimer.Change(1000, Timeout.Infinite);
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
                    if (_backdropRotationTimer == null)
                    {
                        _backdropRotationTimer = new Timer(OnBackdropRotationTimerCallback, null, 8000, 8000);
                    }
                    else
                    {
                        _backdropRotationTimer.Change(8000, 8000);
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
