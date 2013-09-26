using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Osd
{
    /// <summary>
    /// Interaction logic for InfoPanel.xaml
    /// </summary>
    public partial class InfoPanel : UserControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public TransportOsdViewModel ViewModel
        {
            get { return DataContext as TransportOsdViewModel; }
        }

        public InfoPanel()
        {
            InitializeComponent();

            DataContextChanged += FullscreenVideoTransportOsd_DataContextChanged;
            Unloaded += FullscreenVideoTransportOsd_Unloaded;
        }

        void FullscreenVideoTransportOsd_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged += vm_PropertyChanged;
                UpdateNowPlayingItem(vm);
            }
        }

        void FullscreenVideoTransportOsd_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= vm_PropertyChanged;
            }
        }

        void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            if (string.Equals(e.PropertyName, "NowPlayingItem"))
            {
                UpdateNowPlayingItem(vm);
            }
        }

        private async void UpdateImage(TransportOsdViewModel viewModel)
        {
            var media = viewModel.NowPlayingItem;

            ImgPrimary.Visibility = Visibility.Hidden;

            if (media != null && media.HasPrimaryImage)
            {
                try
                {
                    ImgPrimary.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(viewModel.ApiClient.GetImageUrl(media, new ImageOptions
                    {
                        Height = 300

                    }), CancellationToken.None);

                    ImgPrimary.Visibility = Visibility.Visible;
                }
                catch
                {
                    // Already logged at lower levels
                }
            }
        }

        private void UpdateNowPlayingItem(TransportOsdViewModel viewModel)
        {
            UpdateImage(viewModel);

            var media = viewModel.NowPlayingItem;

            TxtName.Text = media == null ? string.Empty : media.Name;
        }
    }
}
