using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Pages;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for FullscreenVideoPage.xaml
    /// </summary>
    public partial class FullscreenVideoPage : BaseFullscreenVideoPage
    {
        public FullscreenVideoPage(IUserInputManager userInputManager)
            : base(userInputManager)
        {
            InitializeComponent();
        }
    }
}
