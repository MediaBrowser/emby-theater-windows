using System.Threading;
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

namespace MediaBrowser.Plugins.DefaultTheme.Screensavers
{
    /// <summary>
    /// Screen saver factory to create Backdrop Screen saver
    /// </summary>
    public class BackdropScreensaverFactory : IScreensaverFactory
    {
        private readonly IApplicationHost _applicationHost;
      
        public BackdropScreensaverFactory(IApplicationHost applicationHost)
        {
            _applicationHost = applicationHost;
        }

        public string Name { get { return "Backdrop"; } }

        public IScreensaver GetScreensaver()
        {
            return (IScreensaver)_applicationHost.CreateInstance(typeof(BackdropScreensaverWindow));
        }
    }
   
    /// <summary>
    /// Interaction logic for ScreensaverWindow.xaml
    /// </summary>
    public partial class BackdropScreensaverWindow : ScreensaverWindowBase
    {
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public BackdropScreensaverWindow(ISessionManager session, IApiClient apiClient, IPresentationManager presentationManager, IScreensaverManager screensaverManager, IImageManager imageManager, ILogger logger)
            : base(presentationManager, screensaverManager, logger)
        {
            
            _session = session;
            _apiClient = apiClient;
            _imageManager = imageManager;
            InitializeComponent();

            DataContext = this;

            LoadScreensaver();
        }
        
        private  async void LoadScreensaver()
        {
            MainGrid.Children.Clear();

           
            var items = await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _session.CurrentUser.Id,
                ImageTypes = new[] { ImageType.Backdrop },
                IncludeItemTypes = new[] { "Movie", "Boxset", "Trailer", "Game", "Series", "MusicArtist" },
                Limit = 1000,
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true
            }, CancellationToken.None);

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
                    ImageType = ImageType.Backdrop
                })
            });

            MainGrid.Children.Add(new ImageViewerControl
            {
                DataContext = new ImageViewerViewModel(_imageManager, images)
                {
                    ImageStretch = Stretch.UniformToFill
                }
            });
            
        }
   }
}
