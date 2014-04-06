using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
using Image = System.Windows.Controls.Image;

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
        private const double MaxPhotoWidth = 1000;      // in photo pixels
        private const double MaxPhotoHeight = 1000;
        private const double MaxPhotoScaleFactor = 3.5; // how many maxwid+th photos should fixt across the screen
        private const double PhotoScaleFactor = 4; // photo sizes  are rand-omly scaled by max b  1.0..4.0
        private const int NumLanes =  8;
        private const int MinAnimateTimeSecs = 4;
        private const int MaxAnimateTimeSecs = 30;

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

            //Timeline.DesiredFrameRateProperty.OverrideMetadata(
            //  typeof(Timeline),
            //  new FrameworkPropertyMetadata { DefaultValue = 40 });

            for (var i = 0; i < _animations.Length; i++)
            {
                _animations[i] = new DoubleAnimation();
               
            }

            DataContext = this;
            ScreensaverCanvas.LayoutUpdated += ScreensaverCanvas_LayoutUpdated;

            this.Loaded += PhotoScreensaverWindow_Loaded;
        }

        // Orientations.
        private const int OrientationId = 0x0112;
        public enum ExifOrientations : byte
        {
            Unknown = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomRight = 3,
            BottomLeft = 4,
            LeftTop = 5,
            RightTop = 6,
            RightBottom = 7,
            LeftBottom = 8,
        }

        public enum Rotation
        {
            D0,
            D90,
            D180,
            D270
        }
         private const string OrientationQuery = "System.Photo.Orientation";

        // Return the image's orientation.
         public static Rotation ImageOrientation(BitmapImage bitmapImage)
        {
          
            var bitmapMetadata = bitmapImage.Metadata as BitmapMetadata;

            if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(OrientationQuery)))
            {
                var orien = bitmapMetadata.GetQuery(OrientationQuery);

                if (orien != null)
                {
                    switch ((PhotoScreensaverWindow.ExifOrientations) orien)
                    {
                        case ExifOrientations.RightTop:
                            return Rotation.D90;
                        case ExifOrientations.BottomRight:
                            return Rotation.D180;
                        case ExifOrientations.LeftBottom:
                            return Rotation.D270;
                    }
                }
            }

             return Rotation.D0;
        }

        private void ScreensaverCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            _canvasWidth = (int)ScreensaverCanvas.ActualWidth;
            _canvasHeight = (int)ScreensaverCanvas.ActualHeight;

            // we only scale one axis, leave other dependant to maintain aspect ratio
            _canvasScaleX = (_canvasWidth / MaxPhotoScaleFactor) / MaxPhotoWidth;
            _canvasScaleY = (_canvasHeight / MaxPhotoScaleFactor) / MaxPhotoHeight;  
        }

        private int _currentIndex = 0;
        private int CurrentIndex { get { return _currentIndex; } set { _currentIndex = value < _photos.Length ? value : 0; } }

        private void PhotoScreensaverWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadScreensaver();
        }

       
        private double MaxWidth(double aspectRatio)
        {
            double maxWidth;

            if (aspectRatio >= 1)
            {
                maxWidth = MaxPhotoWidth*_canvasScaleX;
            }
            else
            {
                var maxHeight = MaxPhotoHeight*_canvasScaleY;
                maxWidth = maxHeight * aspectRatio;
            }

            return maxWidth;
        }

        private double MaxHeight(double aspectRatio)
        {
            double maxHeight;

            if (aspectRatio >= 1)
            {
                var maxWidth = MaxPhotoHeight * _canvasScaleY;
                maxHeight = maxWidth * aspectRatio;
            }
            else
            {
                maxHeight = MaxPhotoHeight * _canvasScaleY;
            }

            return maxHeight;
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
                Limit = 2, //MaxPhotos,
               // SortBy = new[] { ItemSortBy.Random },
                Recursive = true,
                NameStartsWith = "SCD1001",
               
            });

            var ars = items.Items.Select(i => i.PrimaryImageAspectRatio).ToArray();

            _photos  = items.Items.Select(i => new ImageViewerImage
            {
                Caption = i.Name,
                AspectRatio = i.OriginalPrimaryImageAspectRatio??1.0,
                Url = _apiClient.GetImageUrl(i, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    MaxHeight = (int?)MaxHeight(i.OriginalPrimaryImageAspectRatio ?? 1.0),
                    MaxWidth = (int?)MaxWidth(i.OriginalPrimaryImageAspectRatio ?? 1.0),
                    Quality = 100
                })
            }).ToArray();

            return true;
        }

   
        private async Task<Image> GetNextPhoto(string url)
        {
            CurrentIndex++;
            var imageDownloadCancellationTokenSource = new CancellationTokenSource();
            var photo = await _imageManager.GetRemoteImageAsync(url, imageDownloadCancellationTokenSource.Token);
            imageDownloadCancellationTokenSource.Token.ThrowIfCancellationRequested();

            return photo;
        }

        public async Task<System.IO.Stream> GetBitmapStream(string url)
        {
            return await Task.Run(() => _apiClient.GetImageStreamAsync(url));
        }

        // return a range 1..PhotoScaleFactor
        private double RadomPhotoScale()
        {
            var sf = 1 + (_random.NextDouble()*(PhotoScaleFactor - 1));
            return sf;
        }

        private  async Task<BitmapImage> GetNextPhotoBitmap()
        {
            CurrentIndex++;
            var url = _photos[CurrentIndex].Url;
            var aspectRatio = _photos[CurrentIndex].AspectRatio;
            var cancellationToken = new CancellationToken();
            var httpStream =  await GetBitmapStream(url);
            cancellationToken.ThrowIfCancellationRequested();

            int widthPixels, heightPixels;
            if (aspectRatio >= 1.0)
            {
                widthPixels = (int) ((MaxPhotoWidth *_canvasScaleX) / RadomPhotoScale());
                heightPixels = (int) (widthPixels/aspectRatio);
            }
            else
            {
                heightPixels = (int)((MaxPhotoHeight * _canvasScaleY) / RadomPhotoScale());
                widthPixels = (int)(heightPixels * aspectRatio);
            }

            var bitmap = new BitmapImage();
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.Fant);
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = widthPixels;
            bitmap.DecodePixelHeight = heightPixels;
            bitmap.StreamSource = httpStream;
            bitmap.EndInit();

            return bitmap;
        }

        private async Task<DoubleAnimation> StartNextPhotoAnimation(DoubleAnimation animation, int laneNum, bool completionFlag = false)
        {
            var photoindex = CurrentIndex;
            //var photo = await GetNextPhoto();

            var photo = new Image { Source = await GetNextPhotoBitmap() };
            //photo.LayoutTransform = new RotateTransform(45, 25, 25);
           
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
                await StartNextPhotoAnimation(animation, laneNum, true);
            };
            animation.Completed += handler;

            //var r = _random.Next(0, NumLanes - 1);

            //double xStart = ((_canvasWidth - photoWidth)  / (NumLanes - 1)) * r; // and xfinish, stays in one lane
            var r = _random.Next(0, (int)_canvasWidth);
            double xStart = r;
            //_logger.Debug("xxx {0}", photoindex);
            _logger.Debug("{0} {1} Dur {2} ({3},{4}) From {5} - {6}  xStart {7} r {8} Canvas({9}, {10}) {11}", photoindex, laneNum, animation.Duration, (int)photoWidth, (int)photoHeight, (int)animation.From, (int)animation.To, (int)xStart, r, (int) _canvasWidth, (int) _canvasHeight, completionFlag);
            Canvas.SetLeft(photo, xStart);

           // photo.Stretch = Stretch.UniformToFill;
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
              
                await StartNextPhotoAnimation(_animations[i], i);
            }
        }
      
   }
}
