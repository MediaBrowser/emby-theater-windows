using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Pages;
using Microsoft.Expression.Media.Effects;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Theater.Core.ImageViewer
{
    /// <summary>
    /// Interaction logic for ImageViewerPage.xaml
    /// </summary>
    public partial class ImageViewerPage : BasePage, ISupportsThemeMedia
    {
        private readonly IThemeManager _themeManager;

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

        /// <summary>
        /// Gets or sets the random.
        /// </summary>
        /// <value>The random.</value>
        private readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

        public ImageViewerPage(IThemeManager themeManager, ImageViewerViewModel viewModel)
        {
            _themeManager = themeManager;
            InitializeComponent();

            DataContext = viewModel;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += ImageViewerPage_Loaded;
            Unloaded += ImageViewerPage_Unloaded;
        }

        async void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "CurrentImage"))
            {
                await Task.Delay(500);

                TransitionControl.TransitionType = _effects[_random.Next(0, _effects.Length)];
            }
        }

        void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ImageViewerViewModel)DataContext;

            viewModel.PropertyChanged -= vm_PropertyChanged;
            viewModel.StopRotating();
            _themeManager.CurrentTheme.SetGlobalContentVisibility(true);
        }

        void ImageViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ImageViewerViewModel)DataContext;

            viewModel.PropertyChanged += vm_PropertyChanged;
            viewModel.StartRotating();
            _themeManager.CurrentTheme.SetGlobalContentVisibility(false);
        }
    }
}
