using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows;
using System.Windows.Forms.Integration;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Interaction logic for HiddenWindow.xaml
    /// </summary>
    public partial class HiddenWindow : Window, IHiddenWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenWindow" /> class.
        /// </summary>
        public HiddenWindow()
        {
            InitializeComponent();
        }

        WindowsFormsHost IHiddenWindow.WindowsFormsHost
        {
            get { return WindowsFormsHost; }
        }
    }
}
