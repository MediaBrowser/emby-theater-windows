using System;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Theater.Api.Commands;
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
        public bool CloseOnBackCommand { get; set; }
        public bool NavigateBackOnClose { get; set; }

        public PopupWindow(INavigator navigator)
        {
            InitializeComponent();

            NavigateBackOnClose = true;

            EventHandler windowClosed = null;
            windowClosed = (sender, args) => {
                if (NavigateBackOnClose) {
                    navigator.Back();
                }

                Closed -= windowClosed;
            };

            AddHandler(InputCommands.CommandSentEvent, new RoutedEventHandler(CommandSent));

            Closed += windowClosed;
            MouseDown += (s, e) => ClosePopup();
            Loaded += (s, e) => {
                var vm = DataContext as IViewModel;
                if (vm != null) {
                    vm.Closed += ViewModelClosed;
                }
            };
        }

        private void ViewModelClosed(object sender, EventArgs e)
        {
            ClosePopup();
        }

        private void CommandSent(object sender, RoutedEventArgs arg)
        {
            var args = (CommandRoutedEventArgs)arg;
            if (CloseOnBackCommand && args.Command == Command.Back) {
                NavigateBackOnClose = false;
                args.Handled = true;
                ClosePopup();
            }
        }

        public async Task ClosePopup()
        {
            Func<Task> action = async () => {
                var context = DataContext as IViewModel;
                if (context != null) {
                    context.Closed -= ViewModelClosed;

                    if (context.IsActive) {
                        await context.Close();
                    }
                }

                Close();
            };

            await action.OnUiThreadAsync();
        }
    }
}