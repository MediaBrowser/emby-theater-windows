using System.Windows;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.Views
{
    /// <summary>
    ///     Interaction logic for ItemTileView.xaml
    /// </summary>
    public partial class ItemTileView
    {
        public event RoutedEventHandler CommandSent
        {
            add { AddHandler(InputCommands.CommandSentEvent, value); }
            remove { RemoveHandler(InputCommands.CommandSentEvent, value); }
        }

        public ItemTileView()
        {
            InitializeComponent();

            CommandSent += ItemTileView_CommandSent;
            KeyDown += ItemTileView_KeyDown;
        }

        void ItemTileView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var vm = DataContext as ItemTileViewModel;
            if (vm == null) {
                return;
            }

            if (e.Key == System.Windows.Input.Key.Apps || e.Key == System.Windows.Input.Key.OemTilde || e.Key == System.Windows.Input.Key.Oem8) {
                vm.ShowCommandMenuCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void ItemTileView_CommandSent(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ItemTileViewModel;
            var arg = e as CommandRoutedEventArgs;
            if (vm == null || arg == null) {
                return;
            }

            if (arg.Command == Command.Play || arg.Command == Command.PlayPause) {
                vm.PlayCommand.Execute(null);
                e.Handled = true;
            }

            if (arg.Command == Command.Info) {
                vm.ShowCommandMenuCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}