using System.Windows.Media.Animation;
using MediaBrowser.Theater.Presentation.Controls;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.SystemOptionsMenu
{
    public partial class SystemOptionsWindow : BaseModalWindow
    {
        public SystemOptionsWindow()
        {
            InitializeComponent();

            Loaded += SystemOptionsWindow_Loaded;
        }

        protected override void CloseModal()
        {
            base.CloseModal();
        }

        void SystemOptionsWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
    }
}
