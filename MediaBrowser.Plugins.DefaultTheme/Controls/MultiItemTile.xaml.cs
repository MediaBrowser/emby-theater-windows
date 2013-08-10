using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Presentation.ViewModels;
using Microsoft.Expression.Media.Effects;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for MultiItemTile.xaml
    /// </summary>
    public partial class MultiItemTile : UserControl
    {
        /// <summary>
        /// The _image width
        /// </summary>
        private int _imageWidth;
        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image.</value>
        public int ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                MainGrid.Width = value;
            }
        }

        /// <summary>
        /// The _image height
        /// </summary>
        private int _imageHeight;
        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        /// <value>The height of the image.</value>
        public int ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                MainGrid.Height = value;
            }
        }

        public string FixedTitle { get; set; }

        /// <summary>
        /// The _effects
        /// </summary>
        readonly TransitionEffect[] _effects = new TransitionEffect[]
			{ 
				new BlindsTransitionEffect { Orientation = BlindOrientation.Horizontal },
				new BlindsTransitionEffect { Orientation = BlindOrientation.Vertical },
	            new CircleRevealTransitionEffect { },
				new FadeTransitionEffect { },
				new SlideInTransitionEffect {  SlideDirection= SlideDirection.TopToBottom},
				new SlideInTransitionEffect {  SlideDirection= SlideDirection.RightToLeft},
				new WipeTransitionEffect { WipeDirection = WipeDirection.RightToLeft},
				new WipeTransitionEffect { WipeDirection = WipeDirection.TopLeftToBottomRight}
			};

        /// <summary>
        /// Gets or sets the random.
        /// </summary>
        /// <value>The random.</value>
        private Random Random { get; set; }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <value>The collection.</value>
        public RotatingCollectionViewModel ViewModel
        {
            get { return DataContext as RotatingCollectionViewModel; }
        }

        public BaseItemDto CurrentItem
        {
            get { return ViewModel.CurrentItem; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiItemTile" /> class.
        /// </summary>
        public MultiItemTile()
        {
            InitializeComponent();

            Random = new Random(Guid.NewGuid().GetHashCode());

            DataContextChanged += BaseItemTile_DataContextChanged;

            Loaded += MultiItemTile_Loaded;
            Unloaded += MultiItemTile_Unloaded;
        }

        void MultiItemTile_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.StopTimer();
            }
        }

        void MultiItemTile_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.StartTimer();
            }
        }

        /// <summary>
        /// Handles the DataContextChanged event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(OnCurrentItemChanged, DispatcherPriority.Background);

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += Collection_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        void Collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("CurrentItem"))
            {
                Dispatcher.InvokeAsync(OnCurrentItemChanged, DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Called when [current item changed].
        /// </summary>
        private async void OnCurrentItemChanged()
        {
            if (ViewModel == null)
            {
                // Setting this to null doesn't seem to clear out the content
                TileTransitionControl.Content = new FrameworkElement();
                TxtName.Text = FixedTitle;
                return;
            }

            var currentItem = ViewModel.CurrentItem;

            if (currentItem == null)
            {
                // Setting this to null doesn't seem to clear out the content
                TileTransitionControl.Content = new FrameworkElement();
                TxtName.Text = FixedTitle;
                return;
            }

            var img = new Image
            {
                Stretch = Stretch.UniformToFill
            };

            var url = GetImageSource(currentItem);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    img.Source = await ViewModel.ImageManager.GetRemoteBitmapAsync(url);
                    TxtName.Text = FixedTitle ?? GetDisplayName(currentItem);
                }
                catch (HttpException)
                {
                }
            }

            TileTransitionControl.TransitionType = _effects[Random.Next(0, _effects.Length)];
            TileTransitionControl.Content = img;
        }

        internal static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IsType("Episode"))
            {
                name = item.SeriesName;

                if (item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue)
                {
                    name = name + ": " + string.Format("Season {0}, Ep. {1}", item.ParentIndexNumber.Value, item.IndexNumber.Value);
                }

            }

            return name;
        }

        /// <summary>
        /// Gets the image source.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Uri.</returns>
        private string GetImageSource(BaseItemDto item)
        {
            if (item != null)
            {
                if (item.BackdropCount > 0)
                {
                    return ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Backdrop,
                        Height = ImageHeight,
                        Width = ImageWidth
                    });
                }

                if (item.HasThumb)
                {
                    return ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Thumb,
                        Height = ImageHeight,
                        Width = ImageWidth
                    });
                }

                if (item.HasPrimaryImage)
                {
                    return ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Primary,
                        Height = ImageHeight
                    });
                }
            }

            return null;
        }
    }
}
