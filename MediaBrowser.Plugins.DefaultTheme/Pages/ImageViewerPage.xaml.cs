using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using Microsoft.Expression.Media.Effects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for ImageViewerPage.xaml
    /// </summary>
    public partial class ImageViewerPage : BasePage, ISupportsItemThemeMedia
    {
        private const int RotationPeriodMs = 5000;

        private readonly IThemeManager _themeManager;
        private readonly IImageManager _imageManager;

        private readonly string _title;

        private readonly List<string> _urls;

        private Timer CurrentItemTimer { get; set; }

        private readonly object _timerLock = new object();

        private readonly string _ownerItemId;

        /// <summary>
        /// The _effects
        /// </summary>
        readonly TransitionEffect[] _effects = new TransitionEffect[]
			{ 
	            new CircleRevealTransitionEffect {  },
                new RippleTransitionEffect{ },
                 new WaveTransitionEffect{ },
                  new RadialBlurTransitionEffect{ },
                  new SmoothSwirlGridTransitionEffect{  },
				new FadeTransitionEffect {  },
				new WipeTransitionEffect { WipeDirection = WipeDirection.RightToLeft},
				new WipeTransitionEffect { WipeDirection = WipeDirection.TopRightToBottomLeft},
				new WipeTransitionEffect { WipeDirection = WipeDirection.BottomRightToTopLeft},
				new WipeTransitionEffect { WipeDirection = WipeDirection.TopLeftToBottomRight}
			};

        private int _currentItemIndex;

        /// <summary>
        /// Gets or sets the random.
        /// </summary>
        /// <value>The random.</value>
        private readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

        public ImageViewerPage(IThemeManager themeManager, string title, List<string> urls, IImageManager imageManager, string ownerItemId)
        {
            _themeManager = themeManager;
            _title = title;
            _urls = urls;
            _imageManager = imageManager;
            _ownerItemId = ownerItemId;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += ImageViewerPage_Loaded;
            Unloaded += ImageViewerPage_Unloaded;

            TxtTitle.Text = _title;

            Dispatcher.InvokeAsync(UpdateImage, DispatcherPriority.Background);
        }

        /// <summary>
        /// Disposes the timer
        /// </summary>
        private void DisposeTimer()
        {
            lock (_timerLock)
            {
                if (CurrentItemTimer != null)
                {
                    CurrentItemTimer.Dispose();
                    CurrentItemTimer = null;
                }
            }
        }

        /// <summary>
        /// Reloads the timer
        /// </summary>
        private void ReloadTimer()
        {
            if (_urls.Count > 1)
            {
                lock (_timerLock)
                {
                    if (CurrentItemTimer == null)
                    {
                        CurrentItemTimer = new Timer(OnTimerFired, null, TimeSpan.FromMilliseconds(RotationPeriodMs), TimeSpan.FromMilliseconds(RotationPeriodMs));
                    }
                    else
                    {
                        CurrentItemTimer.Change(TimeSpan.FromMilliseconds(RotationPeriodMs), TimeSpan.FromMilliseconds(RotationPeriodMs));
                    }
                }
            }
        }

        private void OnTimerFired(object state)
        {
            var newIndex = _currentItemIndex + 1;

            if (newIndex >= _urls.Count)
            {
                newIndex = 0;
            }

            _currentItemIndex = newIndex;

            Dispatcher.InvokeAsync(UpdateImage, DispatcherPriority.Background);
        }

        private async void UpdateImage()
        {
            TransitionControl.TransitionType = _effects[_random.Next(0, _effects.Length)];
            var image = await _imageManager.GetRemoteImageAsync(_urls[_currentItemIndex]);

            image.Stretch = Stretch.Uniform;

            TransitionControl.Content = image;
        }

        void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeTimer();
            _themeManager.CurrentTheme.SetGlobalContentVisibility(true);
        }

        void ImageViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadTimer();
            _themeManager.CurrentTheme.SetGlobalContentVisibility(false);
        }

        public string ThemeMediaItemId
        {
            get { return _ownerItemId; }
        }
    }
}
