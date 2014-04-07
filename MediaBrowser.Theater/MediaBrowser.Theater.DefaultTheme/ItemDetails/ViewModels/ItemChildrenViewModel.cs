using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ListItemViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;

        public ListItemViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager, INavigator navigator)
        {
            _item = item;

            ImageType[] preferredImageTypes;
            if (item.Type == "Episode") {
                preferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb };
            } else {
                preferredImageTypes = new[] { ImageType.Screenshot, ImageType.Thumb, ImageType.Art, ImageType.Primary };
            }

            Artwork = new ItemArtworkViewModel(item, apiClient, imageManager) {
                DesiredImageHeight = 100,
                DesiredImageWidth = 178,
                PreferredImageTypes = preferredImageTypes
            };

            NavigateCommand = new RelayCommand(arg => navigator.Navigate(Go.To.Item(item)));
        }

        public ItemArtworkViewModel Artwork { get; private set; }

        public ICommand NavigateCommand { get; private set; }

        public string Name
        {
            get { return _item.Name; }
        }

        public string Detail
        {
            get
            {
                if (_item.Type == "Episode") {
                    if (_item.IndexNumber == null) {
                        return null;
                    }

                    if (_item.IndexNumberEnd != null) {
                        return string.Format("S{0}, E{1}-{2}", _item.ParentIndexNumber, _item.IndexNumber, _item.IndexNumberEnd);
                    }

                    return string.Format("S{0}, E{1}", _item.ParentIndexNumber, _item.IndexNumber);
                }

                if (_item.ProductionYear != null) {
                    return _item.ProductionYear.Value.ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }

        public float Rating
        {
            get { return _item.CommunityRating ?? 0; }
        }

        public bool HasRating
        {
            get { return _item.CommunityRating != null; }
        }
    }

    public class ItemChildrenSectionGenerator
        : BaseItemsListSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;

        public ItemChildrenSectionGenerator(IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IServerEvents serverEvents)
            : base(apiClient, sessionManager, imageManager, navigator, serverEvents)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
        }

        public override bool HasSection(BaseItemDto item)
        {
            return item != null && item.IsFolder;
        }

        public override async Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            ItemsResult result = await Query(item, _apiClient, _sessionManager);
            return new[] { await GetItemsSection(result) };
        }

        public static Task<ItemsResult> Query(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager)
        {
            var query = new ItemQuery { ParentId = item.Id, UserId = sessionManager.CurrentUser.Id };
            return apiClient.GetItemsAsync(query);
        }
    }
}