using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.UI.Playback.InternalPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.Collections;
using System.Windows.Interop;
using System.Threading;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.UI.Playback.DirectShow
{
    public class InternalMediaPlayerDS : BaseInternalMediaPlayer
    {
        DShowPlayer VideoPlayer = null;

        public InternalMediaPlayerDS(ILogger logger)
            : base(logger)
        {
            
        }        

        protected override void EnsureMediaPlayerCreated()
        {
            
        }

        public override bool SupportsMultiFilePlayback
        {
            get { return true; }// Not yet really.
        }

        public override string Name
        {
            get { return "Internal Player"; }
        }

        public override bool CanPlay(BaseItemDto item)
        {
            return false;
            //return item.IsVideo || item.IsAudio;
        }

        public override bool CanControlVolume
        {
            get { return true; }
        }

        public override bool CanMute
        {
            get { return true; }
        }

        public override bool CanQueue
        {
            get { return true; }// Not yet really.
        }

        public override bool CanPause
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// Plays the internal.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="options">The options.</param>
        /// <param name="playerConfiguration">The player configuration.</param>
        protected override void PlayInternal(List<BaseItemDto> items, PlayOptions options, PlayerConfiguration playerConfiguration)
        {
            VideoPlayer = new DShowPlayer();
            VideoPlayer.TopLevel = false; // Will not bind to host otherwise.            
            App.Instance.HiddenWindow.WindowsFormsHost.Child = VideoPlayer;

            Uri path;
            string fileName = items.First().Path;// Gotta setup multi file.
            if (Uri.TryCreate(fileName, UriKind.RelativeOrAbsolute, out path))
            {
                VideoPlayer.m_sourceUri = path;
                VideoPlayer.Item = items.First();
                VideoPlayer.Play(App.Instance.HiddenWindow.WindowsFormsHost.Child.Handle);
                base.PlayInternal(items, options, playerConfiguration);
            }
        }

        /// <summary>
        /// Pauses the internal.
        /// </summary>
        /// <returns>Task.</returns>
        protected override Task PauseInternal()
        {
            return Task.Run(() => VideoPlayer.Pause());
        }

        /// <summary>
        /// Uns the pause internal.
        /// </summary>
        /// <returns>Task.</returns>
        protected override Task UnPauseInternal()
        {
            return Task.Run(() => VideoPlayer.Pause());
        }

        /// <summary>
        /// Stops the internal.
        /// </summary>
        /// <returns>Task.</returns>
        protected override Task StopInternal()
        {
            return Task.Run(() => 
                {
                    VideoPlayer.CloseMe();
                });
        }
    }
}
