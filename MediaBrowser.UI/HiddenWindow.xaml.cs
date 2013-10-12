using MediaBrowser.Model.Logging;
using System;
using System.ComponentModel;
using System.Windows;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Interaction logic for HiddenWindow.xaml
    /// </summary>
    public partial class HiddenWindow : Window
    {
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenWindow" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public HiddenWindow(ILogger logger)
        {
            _logger = logger;
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            _logger.Info("Closing");

            base.OnClosing(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closed" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            _logger.Info("Closed");
            
            base.OnClosed(e);
        }
    }
}
