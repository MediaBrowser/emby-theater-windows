using MediaBrowser.Common;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.UI.Playback;
using System;
using System.Collections.Generic;

namespace MediaBrowser.UI.Controller
{
    /// <summary>
    /// This controls application logic as well as server interaction within the UI.
    /// </summary>
    public class UIKernel
    {
        private readonly IApplicationHost _appHost;

        public UIKernel(IApplicationHost appHost)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Can only have one Kernel");
            }
            Instance = this;
            _appHost = appHost;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static UIKernel Instance { get; private set; }

        /// <summary>
        /// Gets the playback manager.
        /// </summary>
        /// <value>The playback manager.</value>
        public PlaybackManager PlaybackManager { get; private set; }

        /// <summary>
        /// Gets the media players.
        /// </summary>
        /// <value>The media players.</value>
        public IEnumerable<BaseMediaPlayer> MediaPlayers { get; private set; }

        public void Init()
        {
            PlaybackManager = (PlaybackManager)_appHost.CreateInstance(typeof(PlaybackManager));
                                                             
            MediaPlayers = _appHost.GetExports<BaseMediaPlayer>();
        }
    }
}
