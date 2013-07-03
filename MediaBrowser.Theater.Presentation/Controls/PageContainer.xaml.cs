using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for PageContainer.xaml
    /// </summary>
    public partial class PageContainer : UserControl
    {
        public PageContainer()
        {
            InitializeComponent();
        }

        public TransitionFrame Frame
        {
            get { return PageFrame; }
        }
    }
}
