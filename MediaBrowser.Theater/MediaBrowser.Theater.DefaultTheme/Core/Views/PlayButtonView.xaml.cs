using System.Windows;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.Views
{
    /// <summary>
    /// Interaction logic for PlayButtonView.xaml
    /// </summary>
    public partial class PlayButtonView
    {
        public PlayButtonView()
        {
            InitializeComponent();
            Loaded += PlayButtonView_Loaded;
        }

        void PlayButtonView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PlayButtonViewModel;
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
