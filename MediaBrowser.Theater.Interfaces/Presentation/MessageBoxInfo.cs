using MediaBrowser.Theater.Interfaces.Theming;
using System.Windows;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Class MessageBoxOptions
    /// </summary>
    public class MessageBoxInfo
    {
        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>The caption.</value>
        public string Caption { get; set; }
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the button.
        /// </summary>
        /// <value>The button.</value>
        public MessageBoxButton Button { get; set; }
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public MessageBoxIcon Icon { get; set; }
        /// <summary>
        /// Gets or sets the timeout ms.
        /// </summary>
        /// <value>The timeout ms.</value>
        public int TimeoutMs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxInfo"/> class.
        /// </summary>
        public MessageBoxInfo()
        {
            Button = MessageBoxButton.OK;
            Icon = MessageBoxIcon.None;
        }
    }
}
