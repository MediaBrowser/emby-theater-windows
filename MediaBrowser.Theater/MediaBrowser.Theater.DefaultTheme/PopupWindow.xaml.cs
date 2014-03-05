using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Presentation.Controls;

namespace MediaBrowser.Theater.DefaultTheme
{
    /// <summary>
    ///     Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : BaseModalWindow
    {
        public PopupWindow(INavigator navigator)
        {
            InitializeComponent();

            MouseDown += (s, e) => navigator.Back();
        }
    }
}