using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Core.ImageViewer;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaBrowser.Theater.Core.Screensaver
{
    /// <summary>
    /// Interaction logic for ScreensaverWindow.xaml
    /// </summary>
    public partial class ScreensaverWindow : BaseModalWindow
    {
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public ScreensaverWindow(ISessionManager session, IApiClient apiClient, IImageManager imageManager)
        {
            _session = session;
            _apiClient = apiClient;
            _imageManager = imageManager;
            InitializeComponent();

            DataContext = this;

            LoadScreensaver();

            Loaded += ScreensaverWindow_Loaded;
            Unloaded += ScreensaverWindow_Unloaded;
        }

        void ScreensaverWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        void ScreensaverWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            CloseModal();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            CloseModal();
        }

        /// <summary>
        /// The _last mouse move point
        /// </summary>
        private Point? _lastMouseMovePoint;

        /// <summary>
        /// Handles OnMouseMove to auto-select the item that's being moused over
        /// </summary>
        /// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseEventArgs" />.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Store the last position for comparison purposes
            // Even if the mouse is not moving this event will fire as elements are showing and hiding
            var pos = e.GetPosition(this);

            if (!_lastMouseMovePoint.HasValue)
            {
                _lastMouseMovePoint = pos;
                return;
            }

            if (pos == _lastMouseMovePoint.Value)
            {
                return;
            }

            CloseModal();
        }

        private void LoadScreensaver()
        {
            MainGrid.Children.Clear();

            if (_session.CurrentUser == null)
            {
                MainGrid.Children.Add(new LogoScreensaver());
            }
            else
            {
                LoadUserScreensaver();
            }
        }

        private async void LoadUserScreensaver()
        {
            var items = await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _session.CurrentUser.Id,
                ImageTypes = new[] { ImageType.Backdrop },
                IncludeItemTypes = new[] { "Movie", "Boxset", "Trailer", "Game", "Series", "MusicArtist" },
                Limit = 100,
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true

            });

            if (items.Items.Length == 0)
            {
                MainGrid.Children.Add(new LogoScreensaver());
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
