using MediaBrowser.Common.Kernel;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.UI.Configuration;
using MediaBrowser.UI.Playback;
using System.Collections.Generic;

namespace MediaBrowser.UI.Controller
{
    /// <summary>
    /// This controls application logic as well as server interaction within the UI.
    /// </summary>
    public class UIKernel : BaseKernel<UIApplicationConfiguration, UIApplicationPaths>
    {
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
        /// Initializes a new instance of the <see cref="UIKernel" /> class.
        /// </summary>
        public UIKernel(IApplicationHost appHost, UIApplicationPaths appPaths, IXmlSerializer xmlSerializer, ILogger logger)
            : base(appHost, appPaths, xmlSerializer, logger)
        {
            Instance = this;
        }

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
                return "http://+:" + Configuration.HttpServerPortNumber + "/mediabrowserui/";
            }
        }

        protected override async void ReloadInternal()
        {
            base.ReloadInternal();

            PlaybackManager = (PlaybackManager)ApplicationHost.CreateInstance(typeof(PlaybackManager));

            Themes = ApplicationHost.GetExports<BaseTheme>();
            MediaPlayers = ApplicationHost.GetExports<BaseMediaPlayer>();
        }
    }
}
