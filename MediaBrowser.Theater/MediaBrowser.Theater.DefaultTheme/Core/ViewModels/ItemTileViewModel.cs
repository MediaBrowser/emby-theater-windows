using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemTileViewModel
        : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IServerEvents _serverEvents;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;

        private readonly BaseItemDto _item;

        public string DisplayName { get; private set; }
        public string Creator { get; private set; }
        public bool HasCreator { get; private set; }
        public ImageViewerViewModel Image { get; private set; }
        public bool IsPlayed { get; private set; }
        public bool IsInProgress { get; private set; }
        public double PlayedPercent { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand GoToDetailsCommand { get; private set; }
        public ICommand PlayTrailerCommand { get; private set; }
    }
}
