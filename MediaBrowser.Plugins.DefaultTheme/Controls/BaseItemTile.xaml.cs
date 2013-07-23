using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Presentation.Extensions;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for BaseItemTile.xaml
    /// </summary>
    public partial class BaseItemTile : UserControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public BaseItemDtoViewModel ViewModel
        {
            get { return DataContext as BaseItemDtoViewModel; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemTile" /> class.
        /// </summary>
        public BaseItemTile()
        {
            InitializeComponent();

            DataContextChanged += BaseItemTile_DataContextChanged;
            Loaded += BaseItemTile_Loaded;
            Unloaded += BaseItemTile_Unloaded;
        }

        /// <summary>
        /// Handles the Unloaded event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the DataContextChanged event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnItemChanged();

            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReloadImage(ViewModel);
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            var viewModel = ViewModel;

            var item = viewModel.Item;

            ReloadImage(viewModel);

            var nameVisibility = !string.Equals(viewModel.ViewType, ViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase) && !item.IsType("Episode") && !item.IsType("Audio") ? Visibility.Collapsed : Visibility.Visible;

            if (item.IsType("Person") || item.IsType("IndexFolder") || viewModel.IsSpecialFeature)
            {
                nameVisibility = Visibility.Visible;
            }

            else if (viewModel.IsLocalTrailer)
            {
                nameVisibility = Visibility.Collapsed;
            }

            TxtName.Visibility = nameVisibility;

            if (nameVisibility == Visibility.Visible)
            {
                TxtName.Text = GetDisplayName(item);
            }

            var progressBarVisibility = Visibility.Collapsed;

            if (item.CanResume && item.RunTimeTicks.HasValue)
            {
                progressBarVisibility = Visibility.Visible;

                Progress.Maximum = item.RunTimeTicks.Value;
                Progress.Value = item.UserData.PlaybackPositionTicks;
            }

            Progress.Visibility = progressBarVisibility;

            var timeVisibility = Visibility.Collapsed;
            if (viewModel.IsSpecialFeature || viewModel.IsLocalTrailer)
            {
                TxtTime.Text = GetMinutesString(item);
                timeVisibility = Visibility.Visible;
            }

            TxtTime.Visibility = timeVisibility;

            OverlayGrid.Visibility = nameVisibility == Visibility.Visible ||
                                     timeVisibility == Visibility.Visible ||
                                     progressBarVisibility == Visibility.Visible
                                         ? Visibility.Visible
                                         : Visibility.Collapsed;

            UpdateUserData(item);
        }

        private void UpdateUserData(BaseItemDto item)
        {
            if (item.LocationType == LocationType.Offline)
            {
                ImgOffline.Visibility = Visibility.Visible;
                ImgPlayed.Visibility = Visibility.Collapsed;
                ImgNew.Visibility = Visibility.Collapsed;
                return;    
            }

            ImgOffline.Visibility = Visibility.Collapsed;
            
            if (item.UserData != null && item.UserData.Played)
            {
                ImgPlayed.Visibility = Visibility.Visible;
                ImgNew.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImgPlayed.Visibility = Visibility.Collapsed;

                if (item.RecentlyAddedItemCount > 0)
                {
                    ImgNew.Visibility = Visibility.Visible;
                    TxtNew.Text = item.RecentlyAddedItemCount + " NEW";
                }
                else if (item.DateCreated.HasValue && (DateTime.UtcNow - item.DateCreated.Value).TotalDays < 14)
                {
                    ImgNew.Visibility = Visibility.Visible;
                    TxtNew.Text = "NEW";
                }
            }
        }

        private string GetMinutesString(BaseItemDto item)
        {
            var time = TimeSpan.FromTicks(item.RunTimeTicks ?? 0);

            return time.ToString(time.TotalHours < 1 ? "m':'ss" : "h':'mm':'ss");
        }

        public static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IndexNumber.HasValue && !item.IsType("season"))
            {
                name = item.IndexNumber + " - " + name;
            }

            return name;
        }

        /// <summary>
        /// Reloads the image.
        /// </summary>
        private async void ReloadImage(BaseItemDtoViewModel viewModel)
        {
            if (viewModel.ImageDisplayWidth.Equals(0) || viewModel.ImageDisplayWidth.Equals(0))
            {
                return;
            }

            await SetImageSource(viewModel);
        }

        private static readonly Task TrueTaskResult = Task.FromResult(true);

        private Task SetImageSource(BaseItemDtoViewModel viewModel)
        {
            var item = ViewModel.Item;
            
            if (string.Equals(viewModel.ViewType, ViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase))
            {
                if (item.BackdropCount > 0)
                {
                    ItemImage.Stretch = Stretch.UniformToFill;

                    var url = viewModel.GetImageUrl(ImageType.Backdrop);

                    return SetImage(viewModel, url);
                }
                if (item.HasThumb)
                {
                    ItemImage.Stretch = Stretch.UniformToFill;

                    var url = viewModel.GetImageUrl(ImageType.Thumb);

                    return SetImage(viewModel, url);
                }
            }

            if (item.HasPrimaryImage)
            {
                ItemImage.Stretch = GetStretchForPrimaryImage(viewModel);

                var url = viewModel.GetImageUrl(ImageType.Primary);

                return SetImage(viewModel, url);
            }

            SetDefaultImage(viewModel);

            return TrueTaskResult;
        }

        private Stretch GetStretchForPrimaryImage(BaseItemDtoViewModel viewModel)
        {
            if (viewModel.IsCloseToMedianPrimaryImageAspectRatio)
            {
                return Stretch.UniformToFill;
            }
            return Stretch.Uniform;
        }

        private async Task SetImage(BaseItemDtoViewModel viewModel, string url)
        {
            MainGrid.Background = null;

            try
            {
                ItemImage.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(url);
            }
            catch (HttpException)
            {
                SetDefaultImage(viewModel);
            }
        }

        /// <summary>
        /// Sets the default image.
        /// </summary>
        private void SetDefaultImage(BaseItemDtoViewModel viewModel)
        {
            var item = ViewModel.Item;

            if (item.IsAudio || item.IsType("MusicAlbum") || item.IsType("MusicArtist"))
            {
                var imageUri = new Uri("../Resources/Images/AudioDefault.png", UriKind.Relative);

                ItemImage.Source = viewModel.ImageManager.GetBitmapImage(imageUri);
            }
            else
            {
                var imageUri = new Uri("../Resources/Images/VideoDefault.png", UriKind.Relative);

                ItemImage.Source = viewModel.ImageManager.GetBitmapImage(imageUri);
            }
        }
    }
}
