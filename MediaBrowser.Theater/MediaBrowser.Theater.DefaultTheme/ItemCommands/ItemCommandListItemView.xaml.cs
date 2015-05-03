using System.Windows;

namespace MediaBrowser.Theater.DefaultTheme.ItemCommands
{
    /// <summary>
    ///     Interaction logic for ItemCommandListItemView.xaml
    /// </summary>
    public partial class ItemCommandListItemView
    {
        public ItemCommandListItemView()
        {
            InitializeComponent();
            Loaded += ItemCommandListItemView_Loaded;
        }

        private void ItemCommandListItemView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ItemCommandListItemViewModel;
            if (vm != null && vm.AutoFocus) {
                if (Button.IsLoaded) {
                    Button.Focus();
                } else {
                    Button.Loaded += (s, arg) => Button.Focus();
                }

                vm.AutoFocus = false;
            }
        }
    }
}