using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class WatchedItemCommand : IItemCommand
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;

        public WatchedItemCommand(IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public async Task Initialize(BaseItemDto item)
        {
            IsEnabled = true;
            DisplayName = "Toggle Watched";
            IconViewModel = new WatchedItemCommandViewModel(item);
            ExecuteCommand = new RelayCommand(o => {
                IApiClient api = _connectionManager.GetApiClient(item);

                if (item.UserData.Played) {
                    api.MarkUnplayedAsync(item.Id, _sessionManager.CurrentUser.Id);
                    item.UserData.Played = true;
                } else {
                    api.MarkPlayedAsync(item.Id, _sessionManager.CurrentUser.Id, null);
                    item.UserData.Played = false;
                }
            });
        }

        public bool IsEnabled { get; private set; }
        public string DisplayName { get; private set; }
        public ICommand ExecuteCommand { get; private set; }
        public IViewModel IconViewModel { get; private set; }

        public int SortOrder
        {
            get { return 80; }
        }
    }
}