using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.DefaultTheme.Osd.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.Views
{
    /// <summary>
    ///     Interaction logic for AudioTrackView.xaml
    /// </summary>
    public partial class AudioTrackView : UserControl
    {
        public AudioTrackView()
        {
            InitializeComponent();

            Loaded += AudioTrackView_Loaded;
        }

        private void AudioTrackView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AudioTrackViewModel;
            if (vm != null && vm.IsPlaying) {
                Button.Focus();
            }
        }
    }
}