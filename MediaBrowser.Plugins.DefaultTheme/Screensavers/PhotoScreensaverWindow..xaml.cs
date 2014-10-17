using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;

namespace MediaBrowser.Plugins.DefaultTheme.Screensavers
{

    /// <summary>
    /// Screen saver factory to create Photo Screen saver
    /// </summary>
    public class PhotoScreensaverFactory // : IScreensaverFactory not ready for public comsumption
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
        private double _maxRenderWidth;
        private double _maxRenderHeight;

        // gloabl behaviour parameters
        private const int MaxPhotos = 50;
        private const double MaxDownloadWidth = 100;        // in photo pixels
        private const double MaxDownloadHeight = 100;
        private const double MaxPhotoScaleFactorWidth = 4;  // how many maxwidth photos should fixt across the screen
        private const double MaxPhotoScaleFactorHeight = 2; // how many maxwidth photos should fixt across the screen
        private const double PhotoScaleFactor = 3.5;        // photo sizes  are rand-omly scaled by max b  1.0..4.0
        private const int NumAnimations = 8;
        private const int MinAnimateTimeSecs = 6;
        private const int MaxAnimateTimeSecs = 30;

        private readonly DoubleAnimation[] _animations = new DoubleAnimation[NumAnimations];
        private readonly Image[] _animatedImages = new Image[NumAnimations];
        private ImageViewerImage[] _photos;
        private readonly Random _random = new Random();
        private readonly GaussianRandom _gaussianRandom = new GaussianRandom();

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

            DataContext = this;
            ScreensaverCanvas.LayoutUpdated += ScreensaverCanvas_LayoutUpdated;

            this.Loaded += PhotoScreensaverWindow_Loaded;
            this.Unloaded += PhotoScreensaverWindow_Unloaded;

        }

        void ScreensaverCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            _canvasWidth = (int)ScreensaverCanvas.ActualWidth;
            _canvasHeight = (int)ScreensaverCanvas.ActualHeight;

            // We calculate both x&y, but when we create the photo bitmap
            // we only scale in one dimension per photo, and cal other dependant to maintain aspect ratio
            _maxRenderWidth = _canvasWidth / MaxPhotoScaleFactorWidth;
            _maxRenderHeight = _maxRenderWidth; //_canvasHeight / MaxPhotoScaleFactorHeight;  
        }

        private int _currentIndex = 0;
        private int CurrentIndex { get { return _currentIndex; } set { _currentIndex = value < _photos.Length ? value : 0; } }

        private void PhotoScreensaverWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadScreensaver();
        }


        private void PhotoScreensaverWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            UnLoadScreensaver();
        }

        private double MaxDownloadWidthAR(double aspectRatio)
        {
            double maxWidth;

            if (aspectRatio >= 1)
            {
                maxWidth = MaxDownloadWidth;
            }
            else
            {
                maxWidth = MaxDownloadHeight * aspectRatio;
            }

            return maxWidth;
        }

        private double MaxDownloadHeightAR(double aspectRatio)
        {
            double maxHeight;

            if (aspectRatio >= 1)
            {
                maxHeight = MaxDownloadWidth / aspectRatio;
            }
            else
            {
                maxHeight = MaxDownloadHeight;
            }

            return maxHeight;
        }


        //
        // Get photo metatdata and convert it to urls for Photo
        // We are not getting the image data itself
        //
        private async Task<bool> GetPhotoData()
        {
            var items = await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _session.LocalUserId,
                ImageTypes = new[] { ImageType.Primary },
                IncludeItemTypes = new[] { "Photo" },
                Fields = new[] { ItemFields.PrimaryImageAspectRatio },
                Limit = 20, //MaxPhotos,
                // SortBy = new[] { ItemSortBy.Random },
                Recursive = true,
            }, CancellationToken.None);

            var ars = items.Items.Select(i => i.PrimaryImageAspectRatio).ToArray();

            _photos = items.Items.Select(i => new ImageViewerImage
            {
                Caption = i.Name,
                AspectRatio = i.OriginalPrimaryImageAspectRatio ?? 1.0,
                Url = _apiClient.GetImageUrl(i, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    MaxWidth = (int?)MaxDownloadWidthAR(i.OriginalPrimaryImageAspectRatio ?? 1.0),
                    MaxHeight = (int?)MaxDownloadHeightAR(i.OriginalPrimaryImageAspectRatio ?? 1.0),
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
            //var sf = 1 + (_random.NextDouble() * (PhotoScaleFactor - 1));
            var sf = 1 + _gaussianRandom.NextGaussian(2);
            _logger.Debug("Gaussian {0}", sf);
            return sf;
        }

        private async Task<BitmapImage> GetNextPhotoBitmap()
        {
            CurrentIndex++;
            var url = _photos[CurrentIndex].Url;
            var aspectRatio = _photos[CurrentIndex].AspectRatio;
            var cancellationToken = new CancellationToken();
            var httpStream = await GetBitmapStream(url);
            cancellationToken.ThrowIfCancellationRequested();


            // calculate the actaul render (pixels) size. Idpendant of  download size
            int widthPixels, heightPixels;
            if (aspectRatio >= 1.0)
            {
                widthPixels = (int)(_maxRenderWidth / RadomPhotoScale());
                heightPixels = (int)(widthPixels / aspectRatio);
            }
            else
            {
                heightPixels = (int)(_maxRenderHeight / RadomPhotoScale());
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

        private async Task<DoubleAnimation> StartNextPhotoAnimation(DoubleAnimation animation, Image animatedImage, bool completionFlag = false)
        {
            var photoindex = CurrentIndex;

            animatedImage.Source = await GetNextPhotoBitmap();
            //var photo = new Image { Source = await GetNextPhotoBitmap() };

            var bi = animatedImage.Source as BitmapImage;
            Debug.Assert(bi != null);
            var photoWidth = bi.Width;
            var photoHeight = bi.Height;

            animation.Duration = new Duration(TimeSpan.FromSeconds(_random.Next(MinAnimateTimeSecs, MaxAnimateTimeSecs)));
            animation.From = -photoHeight;                // y start
            animation.To = _canvasHeight + photoHeight;    // y finish

            // Completion handler - remove the image, start the next handler
            EventHandler handler = null;
            handler = async (s, e) =>
            {
                animation.Completed -= handler;
                ScreensaverCanvas.Children.Remove(animatedImage);
                await StartNextPhotoAnimation(animation, animatedImage, true);
            };
            animation.Completed += handler;

            double xStart = _random.Next(0, (int)_canvasWidth - (int)photoWidth); ;

            _logger.Debug("{0} Dur {1} ({2},{3}) From {4} - {5}  xStart {6} Canvas({7}, {8}) {9}", photoindex, animation.Duration, (int)photoWidth, (int)photoHeight, (int)animation.From, (int)animation.To, (int)xStart, (int)_canvasWidth, (int)_canvasHeight, completionFlag);
            Canvas.SetLeft(animatedImage, xStart);

            // photo.Stretch = Stretch.UniformToFill;
            //animatedImage.Name = "Photo" + CurrentIndex;

            ScreensaverCanvas.Children.Add(animatedImage);
            animatedImage.BeginAnimation(Canvas.TopProperty, animation, HandoffBehavior.SnapshotAndReplace);

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

            //Timeline.DesiredFrameRateProperty.OverrideMetadata(
            //  typeof(Timeline),
            //  new FrameworkPropertyMetadata { DefaultValue = 40 });

            CurrentIndex = -1;
            for (var i = 0; i < _animations.Length; i++)
            {
                if (i >= _photos.Length)
                    continue;

                _animations[i] = new DoubleAnimation();
                _animatedImages[i] = new Image();
                await StartNextPhotoAnimation(_animations[i], _animatedImages[i]);
            }
        }

        private async void UnLoadScreensaver()
        {
            for (var i = 0; i < _animations.Length; i++)
            {
                if (_animatedImages[i] != null)
                {
                    _animatedImages[i].BeginAnimation(Canvas.TopProperty, null);
                    _animatedImages[i] = null;
                    _animations[i] = null;
                }
            }
        }

    }

    public sealed class GaussianRandom
    {
        private bool _hasDeviate;
        private double _storedDeviate;
        private readonly Random _random;

        public GaussianRandom(Random random = null)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// Obtains normally (Gaussian) distributed random numbers, using the Box-Muller
        /// transformation.  This transformation takes two uniformly distributed deviates
        /// within the unit circle, and transforms them into two independently
        /// distributed normal deviates.
        /// </summary>
        /// <param name="mu">The mean of the distribution.  Default is zero.</param>
        /// <param name="sigma">The standard deviation of the distribution.  Default is one.</param>
        /// <returns></returns>
        public double NextGaussian(double mu = 0, double sigma = 1)
        {
            if (sigma <= 0)
                throw new ArgumentOutOfRangeException("sigma", "Must be greater than zero.");

            if (_hasDeviate)
            {
                _hasDeviate = false;
                return _storedDeviate * sigma + mu;
            }

            double v1, v2, rSquared;
            do
            {
                // two random values between -1.0 and 1.0
                v1 = 2 * _random.NextDouble() - 1;
                v2 = 2 * _random.NextDouble() - 1;
                rSquared = v1 * v1 + v2 * v2;
                // ensure within the unit circle
            } while (rSquared >= 1 || rSquared == 0);

            // calculate polar tranformation for each deviate
            var polar = Math.Sqrt(-2 * Math.Log(rSquared) / rSquared);
            // store first deviate
            _storedDeviate = v2 * polar;
            _hasDeviate = true;
            // return second deviate
            return v1 * polar * sigma + mu;
        }
    }
}