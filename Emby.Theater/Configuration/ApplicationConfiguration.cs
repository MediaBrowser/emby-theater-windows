using MediaBrowser.Model.Configuration;
using System.Windows.Forms;

namespace Emby.Theater.Configuration
{
    public class ApplicationConfiguration : BaseApplicationConfiguration
    {
        /// <summary>
        /// Gets or sets the state of the window.
        /// </summary>
        /// <value>The state of the window.</value>
        public FormWindowState? WindowState { get; set; }

        /// <summary>
        /// Gets or sets the window top.
        /// </summary>
        /// <value>The window top.</value>
        public int? WindowTop { get; set; }

        /// <summary>
        /// Gets or sets the window left.
        /// </summary>
        /// <value>The window left.</value>
        public int? WindowLeft { get; set; }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        /// <value>The width of the window.</value>
        public int? WindowWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the window.
        /// </summary>
        /// <value>The height of the window.</value>
        public int? WindowHeight { get; set; }
    }
}
