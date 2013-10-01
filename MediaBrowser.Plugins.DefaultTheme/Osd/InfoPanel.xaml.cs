using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.ComponentModel;
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

        private void UpdateNowPlayingItem(TransportOsdViewModel viewModel)
        {
            var menuViewModel = new InfoPanelViewModel(viewModel, viewModel.ApiClient, viewModel.ImageManager, viewModel.PlaybackManager, viewModel.PresentationManager, viewModel.Logger);

            PageContent.DataContext = MenuList.DataContext = menuViewModel;

            menuViewModel.PropertyChanged += InfoPanel_PropertyChanged;

            var media = viewModel.NowPlayingItem;

            UpdateLogo(viewModel, media);
        }

        void InfoPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "CurrentSection"))
            {
                ScrollViewer.ScrollToLeftEnd();

                var current = MenuList.ItemContainerGenerator.ContainerFromItem(MenuList.SelectedItem) as ListBoxItem;

                if (current != null)
                {
                    current.Focus();
                }
            }
        }

        private async void UpdateLogo(TransportOsdViewModel viewModel, BaseItemDto media)
        {
            ImgLogo.Visibility = Visibility.Hidden;

            if (media != null && (media.HasLogo || !string.IsNullOrEmpty(media.ParentLogoItemId)))
            {
                try
                {
                    ImgLogo.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(viewModel.ApiClient.GetLogoImageUrl(media, new ImageOptions
                    {
                    }));

                    ImgLogo.Visibility = Visibility.Visible;
                }
                catch
                {
                    // Already logged at lower levels
                }
            }
        }

    }
}
