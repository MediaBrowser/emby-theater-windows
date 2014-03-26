using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;

namespace MediaBrowser.UI.EntryPoints
{
    /// <summary>
    /// Class DiskInsertEntryPoint
    /// </summary>
    class DiskInsertEntryPoint : IStartupEntryPoint, IAppFactory, IApp, IDisposable
    {
       
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly ILogManager _logManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackdropsEntryPoint"/> class.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <param name="presentationManager">The presentation manager.</param>
        public DiskInsertEntryPoint(IPresentationManager presentationManager, IPlaybackManager playbackManager, ILogManager logManager)
        {
            _presentationManager = presentationManager;
            _playbackManager = playbackManager;
            _logManager = logManager;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
        }

        /// <summary>
        /// IAppFactory.IAppFactory
        /// </summary>
        public IEnumerable<IApp> GetApps()
        {
            return SystemHasCdRom() ? new List<IApp>() { this } : new List<IApp>();
        }

        /// <summary>
        ///  IApp.Name 
        ///  Gets the name.
        ///  </summary>
        /// <value>The name.</value>
        public string Name { get { return "Disk Insert"; } }

        /// <summary>
        /// IApp.Launch
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Task Launch()
        {

            return Task.Run(() => Play());
        }

        /// <summary>
        /// IApp.Thumb
        /// Gets the thumb image.
        /// </summary>
        /// <returns>FrameworkElement.</returns>
        public FrameworkElement GetThumbImage() { return null; }


        private Boolean SystemHasCdRom()
        {
            return DriveInfo.GetDrives().Any(d => d.DriveType == DriveType.CDRom);
        }

        private void Play()
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.DriveType == DriveType.CDRom);
            Debug.Assert(drive != null); // systemHasCdRom was true earlier
            if (drive.IsReady)
            {
                var item = new BaseItemDto()
                {
                        Id = "-1", 
                        Type="movie", 
                        VideoType = VideoType.Dvd, 
                        Path = drive.Name +  @"\VIDEO_TS",
                        Name=drive.VolumeLabel??String.Empty, 
                        MediaType = "Video"
                };
                _playbackManager.Play(new PlayOptions(item));

                // f:\VIDEO_TS (DVD)
                // f:\BDMV (BD)
            }
            
            /*
            else
            {
                MessageBoxResult msgResult;
                do
                {
                    msgResult = _presentationManager.ShowMessage(new MessageBoxInfo
                    {
                        Button = MessageBoxButton.OKCancel,
                        Caption = "Insert disc",
                        Icon = MessageBoxIcon.Warning,
                        Text = "Insert the disk into the cd/dvd player and ok when ready "
                    });
                } while (msgResult != MessageBoxResult.Cancel && (!drive.IsReady));

                //if (msgResult == MessageBoxResult.Cancel)
                //    return null;

                var page = drive;
            }
            */
            
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
