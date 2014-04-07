using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels;
using MediaBrowser.Theater.DefaultTheme.ItemList;
using MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails
{
    public class ItemDetailsContext
        : NavigationContext
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IPresenter _presenter;
        private readonly ISessionManager _sessionManager;
        private readonly IServerEvents _serverEvents;
        private readonly IEnumerable<IItemDetailSectionGenerator> _generators;

        private ItemDetailsViewModel _viewModel;

        public ItemDetailsContext(IApplicationHost appHost, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, IPresenter presenter, ISessionManager sessionManager)
            : base(appHost)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _presenter = presenter;
            _sessionManager = sessionManager;
            _generators = appHost.GetExports<IItemDetailSectionGenerator>();
        }

        public BaseItemDto Item { get; set; }

        public override async Task Activate()
        {
            if (_viewModel == null || !_viewModel.IsActive) {
                var item = await _apiClient.GetItemAsync(Item.Id, _sessionManager.CurrentUser.Id);

                var allSections = new List<IItemDetailSection>();
                foreach (var generator in _generators.Where(g => g.HasSection(item))) {
                    var sections = await generator.GetSections(item);
                    if (sections != null) {
                        allSections.AddRange(sections);
                    }
                }

                _viewModel = new ItemDetailsViewModel(item, allSections.OrderBy(s => s.SortOrder));
            }

            await _presenter.ShowPage(_viewModel);
        }
    }

    public interface IItemDetailSectionGenerator
    {
        bool HasSection(BaseItemDto item);
        Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item);
    }

    public interface IItemDetailSection
        : IViewModel
    {
        int SortOrder { get; }
    }
}
