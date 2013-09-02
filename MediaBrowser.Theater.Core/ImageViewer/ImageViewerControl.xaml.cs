using MediaBrowser.Theater.Interfaces.ViewModels;
using Microsoft.Expression.Media.Effects;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Core.ImageViewer
{
    /// <summary>
    /// Interaction logic for ImageViewerControl.xaml
    /// </summary>
    public partial class ImageViewerControl : UserControl
    {
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

        public ImageViewerControl()
        {
            InitializeComponent();
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
                await Task.Delay(1000);

                TransitionControl.TransitionType = _effects[_random.Next(0, _effects.Length)];
            }
        }

        void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ImageViewerViewModel)DataContext;

            viewModel.PropertyChanged -= vm_PropertyChanged;
            viewModel.StopRotating();
        }

        void ImageViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ImageViewerViewModel)DataContext;

            viewModel.PropertyChanged += vm_PropertyChanged;
            viewModel.StartRotating();
        }
    }
}
