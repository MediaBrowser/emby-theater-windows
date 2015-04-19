using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class PlayFromStartItemCommand : IItemCommand
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly ISessionManager _sessionManager;

        public PlayFromStartItemCommand(IPlaybackManager playbackManager, IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            _playbackManager = playbackManager;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public async Task Initialize(BaseItemDto item)
        {
            SmartPlayResult<BaseItemDto> items = await item.GetPlayableItems(_connectionManager, _sessionManager);
            BaseItemDto firstItem = items.FirstOrDefault();

            if (firstItem == null) {
                IsEnabled = false;
                return;
            }

            double progress = firstItem.GetPlayedPercent();
            bool resumable = firstItem.CanResume && progress > 0 && progress < 100;

            if (items.IncludesAllChildren && !resumable) {
                IsEnabled = false;
                return;
            }

            IEnumerable<Media> media = items.Select(Media.Create);

            DisplayName = "Play from Start";
            ExecuteCommand = new RelayCommand(o => _playbackManager.Play(media));
            IconViewModel = new PlayFromStartItemCommandView();

            IsEnabled = true;
        }

        public bool IsEnabled { get; private set; }
        public string DisplayName { get; private set; }
        public ICommand ExecuteCommand { get; private set; }
        public IViewModel IconViewModel { get; private set; }
        public int SortOrder { get { return 20; } }
    }
}