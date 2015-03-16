using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.DefaultTheme.Osd.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.Views
{
    /// <summary>
    /// Interaction logic for SubtitleView.xaml
    /// </summary>
    public partial class SubtitleView : UserControl
    {
        public SubtitleView()
        {
            InitializeComponent();

            Loaded += SubtitleView_Loaded;
        }

        void SubtitleView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SubtitleViewModel;
            if (vm != null && vm.IsPlaying) {
                Button.Focus();
            }
        }
    }
}
