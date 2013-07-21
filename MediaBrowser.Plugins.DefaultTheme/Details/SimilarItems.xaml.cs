using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for SimilarItems.xaml
    /// </summary>
    public partial class SimilarItems : BaseItemsControl
    {
        private readonly BaseItemDto _item;

        public SimilarItems(Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow, BaseItemDto item)
            : base(displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            _item = item;
            InitializeComponent();
        }

        protected override ExtendedListBox ItemsList
        {
            get { return LstItems; }
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;
        }

        void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var item = (BaseItemDtoViewModel) e.Argument;

            NavigationManager.NavigateToItem(item.Item, string.Empty);
        }

        protected override Task<ItemsResult> GetItemsAsync()
        {
            var query = new SimilarItemsQuery
                {
                    UserId = SessionManager.CurrentUser.Id,
                    Limit = 12,
                    Fields = new[]
                        {
                                 ItemFields.UserData,
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated,
                                 ItemFields.MediaStreams,
                                 ItemFields.Taglines,
                                 ItemFields.Genres,
                                 ItemFields.SeriesInfo,
                                 ItemFields.Overview,
                                 ItemFields.DisplayPreferencesId
                        },
                        Id = _item.Id
                };

            if (_item.IsType("trailer"))
            {
                return ApiClient.GetSimilarTrailersAsync(query);
            }
            if (_item.IsGame)
            {
                return ApiClient.GetSimilarGamesAsync(query);
            }
            if (_item.IsType("album"))
            {
                return ApiClient.GetSimilarAlbumsAsync(query);
            }
            if (_item.IsType("series"))
            {
                return ApiClient.GetSimilarSeriesAsync(query);
            }

            return ApiClient.GetSimilarMoviesAsync(query);
        }

        protected override double GetImageDisplayHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (medianPrimaryImageAspectRatio.Equals(1))
            {
                medianPrimaryImageAspectRatio = 1.777777777777778;
            }

            double height = displayPreferences.PrimaryImageWidth;

            return height / medianPrimaryImageAspectRatio;
        }

        protected override bool SetBackdropsOnCurrentItemChanged
        {
            get
            {
                return false;
            }
        }
    }
}
