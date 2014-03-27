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


namespace MediaBrowser.Plugins.DefaultTheme.Apps
{
    /// <summary>
    /// Factory to create our IAPP
    /// </summary>
    class PlayExternalDiscAppFactory : IAppFactory
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;


        public PlayExternalDiscAppFactory(IPlaybackManager playbackManager, IImageManager imageManager, ILogManager logManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _logManager = logManager;
        }

        public IEnumerable<IApp> GetApps()
        {
            // dont return an app if we dont have a cdrom dive
            return new [] { SystemHasCdRom() ?  new PlayExternalDiscApp(_playbackManager, _imageManager, _logManager)  : null };
        }

        private Boolean SystemHasCdRom()
        {
            return DriveInfo.GetDrives().Any(d => d.DriveType == DriveType.CDRom);
        }
    }

    /// <summary>
    /// Plays a external DVD or Blueray disk
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
            return Task.Run(() => _playbackManager.PlayExternalDisk(false));
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
