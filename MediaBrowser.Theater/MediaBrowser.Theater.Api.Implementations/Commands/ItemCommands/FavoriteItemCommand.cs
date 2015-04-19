using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class FavoriteItemCommand : BaseViewModel
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;

        public FavoriteItemCommand(IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public bool IsEnabled { get; private set; }
        public string DisplayName { get; private set; }
        public ICommand ExecuteCommand { get; private set; }
        public IViewModel IconViewModel { get; private set; }

        public int SortOrder
        {
            get { return 70; }
        }

        public async Task Initialize(BaseItemDto item)
        {
            IsEnabled = true;
            DisplayName = "Favorite";
            IconViewModel = new DislikeItemCommandViewModel(item);
            ExecuteCommand = new RelayCommand(o => {
                IApiClient api = _connectionManager.GetApiClient(item);

                bool isFavorite = item.UserData.IsFavorite;
                api.UpdateFavoriteStatusAsync(item.Id, _sessionManager.CurrentUser.Id, !isFavorite);
                item.UserData.IsFavorite = !isFavorite;
            });
        }
    }
}