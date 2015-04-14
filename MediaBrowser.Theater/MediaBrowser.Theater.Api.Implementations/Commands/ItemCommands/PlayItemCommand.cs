using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class PlayItemCommand : IItemCommand
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;
        
        public PlayItemCommand(IPlaybackManager playbackManager, IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            _playbackManager = playbackManager;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public async Task Initialize(BaseItemDto item)
        {
            var media = await item.GetSmartPlayMedia(_connectionManager, _sessionManager);
            var firstItem = media.Select(m => m.Item).FirstOrDefault();

            if (firstItem == null) {
                IsEnabled = false;
                return;
            }

            var progress = firstItem.GetPlayedPercent();
            var resumable = firstItem.CanResume && progress > 0 && progress < 100;

            if (firstItem.IsPlayable())
            {
                // single playable item
                DisplayName = resumable ? "Resume" : "Play";
            }
            else
            {
                // folder
                var name = item.GetDisplayName(new DisplayNameFormat(false, true));
                DisplayName = resumable ? string.Format("Resume {0}", name) : string.Format("Play {0}", name);
            }

            ExecuteCommand = new RelayCommand(o => _playbackManager.Play(media));
            IconViewModel = new PlayItemCommandView();

            IsEnabled = true;
        }

        public bool IsEnabled { get; private set; }
        public string DisplayName { get; private set; }
        public ICommand ExecuteCommand { get; private set; }
        public IViewModel IconViewModel { get; private set; }
        public int SortOrder { get; private set; }
    }
}
