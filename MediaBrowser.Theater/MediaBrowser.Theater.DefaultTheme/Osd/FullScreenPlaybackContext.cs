using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Osd.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd
{
    public class FullScreenPlaybackContext
        : NavigationContext
    {
        private readonly ILogger _logger;
        private readonly IPlaybackManager _playbackManager;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IPresenter _presentationManager;
        private readonly INavigator _nav;
        private readonly IServerEvents _serverEvents;
        private readonly IEventAggregator _events;
        private OsdViewModel _viewModel;

        public FullScreenPlaybackContext(ITheaterApplicationHost appHost, IPlaybackManager playbackManager, IApiClient apiClient, IImageManager imageManager, IPresenter presentationManager, ILogManager logManager, INavigator nav, IServerEvents serverEvents, IEventAggregator events)
            : base(appHost)
        {
            _logger = logManager.GetLogger("OSD");
            _playbackManager = playbackManager;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _presentationManager = presentationManager;
            _nav = nav;
            _serverEvents = serverEvents;
            _events = events;
        }

        public override async Task Activate()
        {
            if (_viewModel == null || !_viewModel.IsActive) {
                _viewModel = new OsdViewModel(_playbackManager, _apiClient, _imageManager, _presentationManager, _logger, _nav, _serverEvents, _events);
            }

            await _presentationManager.ShowPage(_viewModel);
        }
    }
}
