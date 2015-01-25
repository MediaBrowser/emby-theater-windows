using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.DefaultTheme.Osd.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.Views
{
    /// <summary>
    ///     Interaction logic for OsdChapterView.xaml
    /// </summary>
    public partial class OsdChapterView
    {
        public OsdChapterView()
        {
            InitializeComponent();

            Loaded += OsdChapterView_Loaded;
        }

        private void OsdChapterView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as OsdChapterViewModel;
            if (vm != null && vm.IsPlaying) {
                Chapters.Button.Focus();
                Chapters.BringIntoView();
            }
        }
    }
}