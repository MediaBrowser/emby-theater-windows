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
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.Osd
{
    public class FullScreenPlaybackContext
        : NavigationContext
    {
        private readonly ILogger _logger;
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly IPresenter _presentationManager;
        private readonly INavigator _nav;
        private readonly IEventAggregator _events;
        private readonly IConnectionManager _connectionManager;
        private OsdViewModel _viewModel;

        public FullScreenPlaybackContext(ITheaterApplicationHost appHost, IPlaybackManager playbackManager, IImageManager imageManager, IPresenter presentationManager, ILogManager logManager, INavigator nav, IEventAggregator events, IConnectionManager connectionManager)
            : base(appHost)
        {
            _logger = logManager.GetLogger("OSD");
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _presentationManager = presentationManager;
            _nav = nav;
            _events = events;
            _connectionManager = connectionManager;
        }

        public override async Task Activate()
        {
            if (_viewModel == null || !_viewModel.IsActive) {
                _viewModel = new OsdViewModel(_playbackManager, _imageManager, _presentationManager, _logger, _nav, _events, _connectionManager);
            }

            await _presentationManager.ShowPage(_viewModel);
        }
    }
}
