using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Osd
{
    /// <summary>
    /// Interaction logic for NowPlayingInfo.xaml
    /// </summary>
    public partial class NowPlayingInfo : UserControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public TransportOsdViewModel ViewModel
        {
            get { return DataContext as TransportOsdViewModel; }
        }

        public NowPlayingInfo()
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

            if (media != null)
            {
                if (media.SeriesPrimaryImageTag.HasValue)
                {
                    try
                    {
                        ImgPrimary.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(viewModel.ApiClient.GetImageUrl(media.SeriesId, new ImageOptions
                        {
                            Height = 350,
                            Tag = media.SeriesPrimaryImageTag,
                            ImageType = ImageType.Primary

                        }), CancellationToken.None);

                        ImgPrimary.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        // Already logged at lower levels
                    }
                }
                else if (media.HasPrimaryImage)
                {
                    try
                    {
                        ImgPrimary.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(viewModel.ApiClient.GetImageUrl(media, new ImageOptions
                        {
                            Height = 600,
                            ImageType = ImageType.Primary

                        }), CancellationToken.None);

                        ImgPrimary.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        // Already logged at lower levels
                    }
                }
            }
        }

        private void UpdateNowPlayingItem(TransportOsdViewModel viewModel)
        {
            UpdateImage(viewModel);

            var media = viewModel.NowPlayingItem;

            TxtName.Text = media == null ? string.Empty : GetName(media);
        }

        private string GetName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IndexNumber.HasValue)
            {
                name = string.Format("Ep. {0} - {1}", item.IndexNumber.Value, name);
            }

            if (item.ParentIndexNumber.HasValue)
            {
                name = string.Format("Season {0}, {1}", item.ParentIndexNumber.Value, name);
            }

            return name;
        }
    }
}
