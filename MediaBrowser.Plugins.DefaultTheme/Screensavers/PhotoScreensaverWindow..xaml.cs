using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using System.Linq;
using System.Windows.Media;
using ServiceStack.Text;

namespace MediaBrowser.Plugins.DefaultTheme.Screensavers
{
    /// <summary>
    /// Screen saver factory to create Photo Screen saver
    /// </summary>
    public class PhotoScreensaverFactory : IScreensaverFactory
    {
        private readonly IApplicationHost _applicationHost;
      

        public PhotoScreensaverFactory(IApplicationHost applicationHost)
        {
            _applicationHost = applicationHost;
        }

        public string Name { get { return "Photo"; } }

        public IScreensaver GetScreensaver()
        {
            return (IScreensaver)_applicationHost.CreateInstance(typeof(PhotoScreensaverWindow));
        }
    }
   
    /// <summary>
    /// Interaction logic for ScreensaverWindow.xaml
    /// </summary>
    public partial class PhotoScreensaverWindow : ScreensaverWindowBase
    {
        private double _canvasWidth;
        private double _canvasHeight;
        private double _canvasScaleX;
        private double _canvasScaleY;

        // gloabl behaviour parameters
        private const int MaxPhotos = 50;
        private const double MaxPhotoWidth = 200;
        private const double MaxPhotoHeight = 200;
        private const double PhotoScaleFactor = 4; // how many maxwidth photos should fixt across the screen
        private const int NumLanes =  8;
        private const int MinAnimateTimeSecs = 4;
        private const int MaxAnimateTimeSecs = 15;

        private readonly DoubleAnimation[] _animations = new DoubleAnimation[NumLanes];
        private ImageViewerImage[] _photos;
        private readonly Random _random = new Random();

        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public PhotoScreensaverWindow(ISessionManager session, IApiClient apiClient, IPresentationManager presentationManager, IScreensaverManager screensaverManager, IImageManager imageManager, ILogger logger)
            : base(presentationManager, screensaverManager, logger)
        {
            _session = session;
            _apiClient = apiClient;
            _imageManager = imageManager;
            InitializeComponent();

            Timeline.DesiredFrameRateProperty.OverrideMetadata(
              typeof(Timeline),
              new FrameworkPropertyMetadata { DefaultValue = 40 });

            for (var i = 0; i < _animations.Length; i++)
            {
                _animations[i] = new DoubleAnimation();
               
            }

            DataContext = this;
            ScreensaverCanvas.LayoutUpdated += ScreensaverCanvas_LayoutUpdated;

            this.Loaded += PhotoScreensaverWindow_Loaded;
        }

        private void ScreensaverCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            _canvasWidth = (int)ScreensaverCanvas.ActualWidth;
            _canvasHeight = (int)ScreensaverCanvas.ActualHeight;

            _canvasScaleX = (_canvasWidth / PhotoScaleFactor) / MaxPhotoWidth;
            _canvasScaleY = _canvasScaleX * (_canvasHeight / _canvasWidth); // maintain aspect ratio
        }

        private int _currentIndex = 0;
        private int CurrentIndex { get { return _currentIndex; } set { _currentIndex = value < _photos.Length ? value : 0; } }

        private void PhotoScreensaverWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadScreensaver();
        }

        //
        // Get photo metatdata and convert it to urls for Photo
        // We are not getting the image data itself
        //
        private async Task<bool> GetPhotoData()
        {
            var items =  await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _session.CurrentUser.Id,
                ImageTypes = new[] { ImageType.Primary },
                IncludeItemTypes = new[] { "Photo" },
                Fields = new[] { ItemFields.PrimaryImageAspectRatio },
                Limit = MaxPhotos,
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true,
            });

            var ars = items.Items.Select(i => i.PrimaryImageAspectRatio).ToArray();

            _photos  = items.Items.Select(i => new ImageViewerImage
            {
                Caption = i.Name,
                Url = _apiClient.GetImageUrl(i, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    MaxHeight = (int?) (MaxPhotoHeight * _canvasScaleY),
                    MaxWidth = (int?)(MaxPhotoWidth * _canvasScaleX),
                    Quality = 20
                })
            }).ToArray();

            return true;
        }

   
        private async Task<Image> GetNextPhoto()
        {
            CurrentIndex++;
            var imageDownloadCancellationTokenSource = new CancellationTokenSource();
            var photo = await _imageManager.GetRemoteImageAsync(_photos[CurrentIndex].Url, imageDownloadCancellationTokenSource.Token);
            imageDownloadCancellationTokenSource.Token.ThrowIfCancellationRequested();

            return photo;
        }

        private async Task<DoubleAnimation> StartNextPhotoAnimation(DoubleAnimation animation, bool completionFlag = false)
        {
            var photoindex = CurrentIndex;
            var photo = await GetNextPhoto();

            var bi = photo.Source as BitmapImage;
            Debug.Assert(bi != null);
            var photoWidth = bi.Width;
            var photoHeight = bi.Height;

            
            animation.Duration = new Duration(TimeSpan.FromSeconds(_random.Next(MinAnimateTimeSecs, MaxAnimateTimeSecs)));
            animation.From = - photoHeight;                // y start
            animation.To = _canvasHeight + photoHeight;    // y finish

            // Completion handler - remove the image, start the next handler
            EventHandler handler = null;
            handler = async (s, e) =>
            {
                animation.Completed -= handler;
                ScreensaverCanvas.Children.Remove(photo);
                StartNextPhotoAnimation(animation, true);
            };
            animation.Completed += handler;

            var r = _random.Next(0, NumLanes - 1);
            double xStart = ((_canvasWidth - photoWidth)  / (NumLanes - 1)) * r; // and xfinish, stays in one lane

            //_logger.Debug("xxx {0}", photoindex);
            _logger.Debug("{0} Dur {1} ({2},{3}) From {4} - {5}  xStart {6} r {7} Canvas({8}, {9}) {10}", photoindex, animation.Duration, (int)photoWidth, (int)photoHeight, (int)animation.From, (int)animation.To, (int)xStart, r, (int) _canvasWidth, (int) _canvasHeight, completionFlag);
            Canvas.SetLeft(photo, xStart);

            photo.Stretch = Stretch.UniformToFill;
            photo.Name = "Photo" + CurrentIndex;

            ScreensaverCanvas.Children.Add(photo);
            photo.BeginAnimation(Canvas.TopProperty, animation, HandoffBehavior.SnapshotAndReplace);

            return animation;
        }

        private async void LoadScreensaver()
        {
            ScreensaverCanvas.Children.Clear();

            await GetPhotoData();
         
            if (_photos.Length == 0)
            {
                _screensaverManager.ShowScreensaver(true, "Logo");
                return;
            }

            
            for (var i = 0; i < NumLanes; i++)
            {
                if (i >= _photos.Length)
                    continue;
                StartNextPhotoAnimation(_animations[i]);
            }
        }
      
   }
}
