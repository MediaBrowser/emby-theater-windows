using System.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MediaBrowser.UI.Implementations
{
    public class RotatingBackdrops : IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private readonly ContentControl _backdropContainer;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;

        private readonly object _rotationTimerLock = new object();
        private readonly object _initialSetTimerLock = new object();

        private DispatcherTimer _backdropSetTimer;
        private DispatcherTimer _backdropRotationTimer;

        /// <summary>
        /// Gets or sets the current backdrops.
        /// </summary>
        /// <value>The current backdrops.</value>
        private string[] _currentBackdrops;
        
        private int _currentBackdropIndex;

        public RotatingBackdrops(Dispatcher dispatcher, ContentControl backdropContainer, IImageManager imageManager, IApiClient apiClient, IPlaybackManager playbackManager)
        {
            _dispatcher = dispatcher;
            _backdropContainer = backdropContainer;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _playbackManager = playbackManager;
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
            var currentBackdropIndex = CurrentBackdropIndex;

            if (currentBackdropIndex == -1)
            {
                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (_backdropContainer.Content != null)
                {
                    _backdropContainer.Content = new FrameworkElement();
                }
                return;
            }

            try
            {
                var bitmap = await _imageManager.GetRemoteBitmapAsync(_currentBackdrops[currentBackdropIndex]);

                var img = new Image
                {
                    Source = bitmap
                };

                img.SetResourceReference(FrameworkElement.StyleProperty, "BackdropImage");

                _backdropContainer.Content = img;
            }
            catch (HttpException)
            {
                if (currentBackdropIndex == 0)
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
                MaxWidth = Convert.ToInt32(SystemParameters.VirtualScreenWidth),
                MaxHeight = Convert.ToInt32(SystemParameters.VirtualScreenHeight)
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
            lock (_initialSetTimerLock)
            {
                _pendingBackdrops = backdrops;

                if (_backdropSetTimer == null)
                {
                    _backdropSetTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Normal,
                                                            OnPendingBackdropsTimerTick, _dispatcher);
                }
                else
                {
                    _backdropSetTimer.Stop();
                    _backdropSetTimer.Start();
                }
            }
        }

        private string[] _pendingBackdrops;
        private void OnPendingBackdropsTimerTick(object sender, EventArgs args)
        {
            DisposeInitialSetTimer();

            SetBackdropsInternal(_pendingBackdrops);
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

            DisposeRotationTimer();
            _currentBackdrops = backdrops;

            if (backdrops == null || backdrops.Length == 0)
            {
                CurrentBackdropIndex = -1;

                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (_backdropContainer.Content != null)
                {
                    _backdropContainer.Content = new FrameworkElement();
                }
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
                        _backdropRotationTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(6000), DispatcherPriority.Normal, OnBackdropRotationTimerCallback, _dispatcher);
                    }
                    else
                    {
                        _backdropRotationTimer.Stop();
                        _backdropRotationTimer.Start();
                    }
                }
            }
        }

        private void OnBackdropRotationTimerCallback(object sender, EventArgs args)
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
                    _backdropRotationTimer.Stop();
                }
            }
        }

        private void DisposeInitialSetTimer()
        {
            lock (_initialSetTimerLock)
            {
                if (_backdropSetTimer != null)
                {
                    _backdropSetTimer.Stop();
                }
            }
        }
    }
}
