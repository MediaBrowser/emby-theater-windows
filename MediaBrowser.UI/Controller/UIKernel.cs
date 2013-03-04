using System;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Kernel;
using MediaBrowser.Model.Logging;
using MediaBrowser.UI.Playback;
using System.Collections.Generic;

namespace MediaBrowser.UI.Controller
{
    /// <summary>
    /// This controls application logic as well as server interaction within the UI.
    /// </summary>
    public class UIKernel : BaseKernel
    {
        private IConfigurationManager _configurationManager;

        public UIKernel(IApplicationHost appHost, ILogManager logManager, IConfigurationManager configurationManager)
            : base(appHost, logManager, configurationManager)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Can only have one Kernel");
            }
            Instance = this;
            _configurationManager = configurationManager;
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

        /// <summary>
        /// Gets the list of currently loaded themes
        /// </summary>
        /// <value>The themes.</value>
        public IEnumerable<BaseTheme> Themes { get; private set; }

        /// <summary>
        /// Gets the kernel context.
        /// </summary>
        /// <value>The kernel context.</value>
        public override KernelContext KernelContext
        {
            get { return KernelContext.Ui; }
        }

        /// <summary>
        /// Gets the UDP server port number.
        /// </summary>
        /// <value>The UDP server port number.</value>
        public override int UdpServerPortNumber
        {
            get { return 7360; }
        }

        /// <summary>
        /// Give the UI a different url prefix so that they can share the same port, in case they are installed on the same machine.
        /// </summary>
        /// <value>The HTTP server URL prefix.</value>
        public override string HttpServerUrlPrefix
        {
            get
            {
                return "http://+:" + _configurationManager.CommonConfiguration.HttpServerPortNumber + "/mediabrowserui/";
            }
        }

        protected override void ReloadInternal()
        {
            base.ReloadInternal();

            PlaybackManager = (PlaybackManager)ApplicationHost.CreateInstance(typeof(PlaybackManager));

            Themes = ApplicationHost.GetExports<BaseTheme>();
            MediaPlayers = ApplicationHost.GetExports<BaseMediaPlayer>();
        }
    }
}
