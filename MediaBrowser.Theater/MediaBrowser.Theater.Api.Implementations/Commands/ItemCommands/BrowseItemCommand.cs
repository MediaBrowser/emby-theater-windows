using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class BrowseItemCommand : IItemCommand
    {
        private readonly IConnectionManager _connectionManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        public BrowseItemCommand(INavigator navigator, IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            _navigator = navigator;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public async Task Initialize(BaseItemDto item)
        {
            bool isFolter = item.IsFolder || item.IsGenre || item.IsPerson || item.IsStudio;

            if (!isFolter) {
                IsEnabled = false;
                return;
            }

            DisplayName = "Browse";
            IconViewModel = new BrowseItemCommandViewModel();
            ExecuteCommand = new RelayCommand(async o => {
                await _navigator.Navigate(Go.To.ItemList(new ItemListParameters {
                    Items = ItemChildren.Get(_connectionManager, _sessionManager, item, new ChildrenQueryParams { ExpandSingleItems = true }),
                    Title = item.GetDisplayName()
                }));
            });

            IsEnabled = true;
        }

        public bool IsEnabled { get; private set; }
        public string DisplayName { get; private set; }
        public ICommand ExecuteCommand { get; private set; }
        public IViewModel IconViewModel { get; private set; }

        public int SortOrder
        {
            get { return 30; }
        }
    }
}