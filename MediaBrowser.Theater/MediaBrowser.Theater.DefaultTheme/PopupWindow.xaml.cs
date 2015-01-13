using System;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.Controls;

namespace MediaBrowser.Theater.DefaultTheme
{
    /// <summary>
    ///     Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : BaseModalWindow
    {
        public bool NavigateBackOnClose { get; set; }

        public PopupWindow(INavigator navigator)
        {
            InitializeComponent();

            EventHandler windowClosed = null;
            windowClosed = (sender, args) => {
                if (NavigateBackOnClose) {
                    navigator.Back();
                }

                Closed -= windowClosed;
            };

            Closed += windowClosed;
            MouseDown += (s, e) => ClosePopup();
        }

        public async Task ClosePopup()
        {
            Func<Task> action = async () => {
                var context = DataContext as IViewModel;
                if (context != null) {
                    await context.Close();
                }

                Close();
            };

            await action.OnUiThreadAsync();
        }
    }
}