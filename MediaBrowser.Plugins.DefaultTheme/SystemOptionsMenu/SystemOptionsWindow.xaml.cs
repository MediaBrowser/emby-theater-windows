using System.Windows.Media.Animation;
using MediaBrowser.Theater.Presentation.Controls;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.SystemOptionsMenu
{
    public partial class SystemOptionsWindow : BaseModalWindow
    {
        public SystemOptionsWindow(DefaultThemePageMasterCommandsViewModel masterCommands)
        {
            InitializeComponent();
            Loaded += SystemOptionsWindow_Loaded;
            masterCommands.PageNavigated += masterCommands_PageNavigated;
            DataContext = masterCommands;
        }

        private void SystemOptionsWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void masterCommands_PageNavigated(object sender, System.EventArgs e)
        {
            CloseModal();
        }
    }
}
