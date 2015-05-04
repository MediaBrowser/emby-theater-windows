using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class LikeItemCommand : IItemCommand
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;

        public LikeItemCommand(IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public async Task Initialize(BaseItemDto item)
        {
            IsEnabled = true;
            DisplayName = "Like";
            IconViewModel = new LikeItemCommandViewModel(item);
            ExecuteCommand = new RelayCommand(o => {
                IApiClient api = _connectionManager.GetApiClient(item);

                if (item.UserData.Likes.HasValue) {
                    api.ClearUserItemRatingAsync(item.Id, _sessionManager.CurrentUser.Id);
                    item.UserData.Likes = null;
                } else {
                    api.UpdateUserItemRatingAsync(item.Id, _sessionManager.CurrentUser.Id, true);
                    item.UserData.Likes = true;
                }
            });
        }

        public bool IsEnabled { get; private set; }
        public string DisplayName { get; private set; }
        public ICommand ExecuteCommand { get; private set; }
        public IViewModel IconViewModel { get; private set; }

        public int SortOrder
        {
            get { return 50; }
        }
    }
}