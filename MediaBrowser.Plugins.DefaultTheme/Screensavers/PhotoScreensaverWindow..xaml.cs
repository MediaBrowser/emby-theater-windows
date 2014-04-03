using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
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

            LoadScreensaver();
        }

        private async void LoadScreensaver()
        {
            MainGrid.Children.Clear();


            var items = await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _session.CurrentUser.Id,
                ImageTypes = new[] { ImageType.Primary },
                IncludeItemTypes = new[] { "Photo" },
                Limit = 6,
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true,
            });

            if (items.Items.Length == 0)
            {
                _screensaverManager.ShowScreensaver(true, "Logo");
                return;
            }

            var images = items.Items.Select(i => new ImageViewerImage
            {
                Caption = i.Name,
                Url = _apiClient.GetImageUrl(i, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    MaxHeight = 100,
                    MaxWidth = 100,
                    Quality = 20
                })
            });


            var imageDownloadCancellationTokenSource = new CancellationTokenSource();
            var c = 0;

            var imgs = new Image[6];
            for (var i = 0; i < imgs.Length; i++)
            {
                _logger.Debug("GetRemoteImageAsync {0}", c++);
                imgs[i] = await _imageManager.GetRemoteImageAsync(images.ElementAt(i).Url, imageDownloadCancellationTokenSource.Token);
                Debug.WriteLine("GetRemoteImageAsync finished {0}", c);
                imageDownloadCancellationTokenSource.Token.ThrowIfCancellationRequested();

                imgs[i].Stretch = Stretch.UniformToFill;
                var s = "x" + i.ToString();
                imgs[i].Name = s;
            }


            var r = new Random();
            var sb = new Storyboard();
            var animations = new DoubleAnimation[4];
            for (var i = 0; i < 4; i++)
            {
                Canvas.SetLeft(imgs[i], 50 + i * 150);

                animations[i] = new DoubleAnimation();
                animations[i].From = -100;
                animations[i].To = 300;
                var rn = r.Next(4, 12);
                var ts = TimeSpan.FromSeconds(rn);
                _logger.Debug("random {0}", ts);
                animations[i].Duration = new Duration(ts);

                //Storyboard.SetTargetName(animations[i], imgs[i].Name);
                Storyboard.SetTarget(animations[i], imgs[i]);
                Storyboard.SetTargetProperty(animations[i], new PropertyPath("(Canvas.Top)"));

                //this.RegisterName(imgs[i].Name, imgs[i]);
                MainGrid.Children.Add(imgs[i]);
                sb.Children.Add(animations[i]);
            }

            animations[1].Completed += PhotoScreensaverWindow_Completed;


            sb.Begin(this);

            MainGrid.MouseDown += (s, e) =>
            {
                sb.Children.Remove(animations[1]);

            };



            //this.UnregisterName("image");

            /*
            MainGrid.Children.Add(new ImageViewerControl
            {
                DataContext = new ImageViewerViewModel(_imageManager, images)
                {
                    ImageStretch = Stretch.UniformToFill
                }
            });
            */
        }

        void PhotoScreensaverWindow_Completed(object sender, EventArgs e)
        {
            var ac = sender as AnimationClock;

            var targetName = Storyboard.GetTargetName(((sender as AnimationClock).Timeline as AnimationTimeline));
            var target = Storyboard.GetTarget(((sender as AnimationClock).Timeline as AnimationTimeline));

            //MainGrid.Children.Remove(target.);
        }
   }
}
