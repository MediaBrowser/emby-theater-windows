using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Osd
{
    public class InfoPanelViewModel : TabbedViewModel
    {
        private readonly TransportOsdViewModel _transportViewModel;

        private IApiClient ApiClient { get; set; }
        private IImageManager ImageManager { get; set; }
        private IPlaybackManager PlaybackManager { get; set; }
        private IPresentationManager PresentationManager { get; set; }
        private ILogger Logger { get; set; }
        
        public InfoPanelViewModel(TransportOsdViewModel transportViewModel, IApiClient apiClient, IImageManager imageManager, IPlaybackManager playbackManager, IPresentationManager presentationManager, ILogger logger)
        {
            _transportViewModel = transportViewModel;
            Logger = logger;
            PresentationManager = presentationManager;
            PlaybackManager = playbackManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
        }

        protected override Task<IEnumerable<TabItem>> GetSections()
        {
            var list = new List<TabItem>();

            list.Add(new TabItem
            {
                DisplayName = "Info",
                Name = "Info"
            });

            if (_transportViewModel.CanSeek && _transportViewModel.SupportsChapters)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Scenes",
                    Name = "Scenes"
                });
            }

            if (_transportViewModel.CanSelectAudioTrack)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Audio",
                    Name = "Audio"
                });
            }

            if (_transportViewModel.CanSelectSubtitleTrack)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Subtitles",
                    Name = "Subtitles"
                });
            }

            return Task.FromResult<IEnumerable<TabItem>>(list);
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            if (string.Equals(section, "Scenes"))
            {
                return _transportViewModel.CreateChaptersViewModel();
            }

            return new TransportOsdViewModel(PlaybackManager, ApiClient, ImageManager, PresentationManager, Logger);
        }
    }
}
