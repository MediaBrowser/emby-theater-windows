using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Session;


namespace MediaBrowser.Plugins.DefaultTheme.Apps
{
    /// <summary>
    /// Factory to create our external disc app
    /// </summary>
    class PlayExternalDiscAppFactory : IAppFactory
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly ITheaterConfigurationManager _theaterConfigurationManager;
        private readonly ILogManager _logManager;


        public PlayExternalDiscAppFactory(ISessionManager sessionManager, IPlaybackManager playbackManager, ITheaterConfigurationManager theaterConfigurationManager, IImageManager imageManager, ILogManager logManager)
        {
            _playbackManager = playbackManager;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _theaterConfigurationManager = theaterConfigurationManager;
            _logManager = logManager;
        }

        public IEnumerable<IApp> GetApps()
        {
            // dont return an app if we dont have a cdrom dive or if we are configured not to show
            var conf = _theaterConfigurationManager.GetUserTheaterConfiguration(_sessionManager.LocalUserId);

            if (conf.ShowExternalDiscApp && SystemHasCdRom())
                return new List<IApp> { new PlayExternalDiscApp(_playbackManager, _imageManager, _logManager) };
            else
                return new List<IApp>();
       }

        private Boolean SystemHasCdRom()
        {
            return DriveInfo.GetDrives().Any(d => d.DriveType == DriveType.CDRom);
        }
    }

    /// <summary>
    /// Plays a external DVD or Blueray disc
    /// </summary>
    class PlayExternalDiscApp : IApp
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;

        public PlayExternalDiscApp(IPlaybackManager playbackManager, IImageManager imageManager, ILogManager logManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _logManager = logManager;
        }

        public string Name { get { return "Play External Disc"; } }

        public Task Launch()
        {
            return Task.Run(() => _playbackManager.PlayExternalDisc(false));
        }

        public static Uri GetThumbUri()
        {
            return new  Uri("pack://application:,,,/MediaBrowser.Plugins.DefaultTheme;component/Resources/Images/insertDisc.png", UriKind.Absolute);
        }

        public FrameworkElement GetThumbImage()
        {
            var x = GetThumbUri();

            var image = new Image
            {
                Source = _imageManager.GetBitmapImage(GetThumbUri())
            };

            return image;
        }

        public void Dispose()
        {
        }

    }

}
