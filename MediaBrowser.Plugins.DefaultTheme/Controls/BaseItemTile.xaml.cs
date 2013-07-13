using System.Windows.Media;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        private BaseItemDto Item
        {
            get { return ViewModel.Item; }
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
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReloadImage(Item);
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            var item = Item;

            ReloadImage(item);

            var nameVisibility = !string.Equals(ViewModel.ViewType, ViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase) && !Item.IsType("Episode") && !Item.IsType("Audio") ? Visibility.Collapsed : Visibility.Visible;

            if (item.IsType("Person") || item.IsType("IndexFolder") || ViewModel.IsSpecialFeature)
            {
                nameVisibility = Visibility.Visible;
            }

            else if (ViewModel.IsLocalTrailer)
            {
                nameVisibility = Visibility.Collapsed;
            }

            TxtName.Visibility = nameVisibility;

            if (nameVisibility == Visibility.Visible)
            {
                TxtName.Text = GetDisplayName(Item);
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
            if (ViewModel.IsSpecialFeature || ViewModel.IsLocalTrailer)
            {
                TxtTime.Text = GetMinutesString(item);
                timeVisibility = Visibility.Visible;
            }

            TxtTime.Visibility = timeVisibility;


            var personRoleVisibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(ViewModel.PersonRole))
            {
                TxtRole.Text = "as " + ViewModel.PersonRole;
                personRoleVisibility = Visibility.Visible;
            }

            TxtRole.Visibility = personRoleVisibility;

            OverlayGrid.Visibility = nameVisibility == Visibility.Visible ||
                                     timeVisibility == Visibility.Visible ||
                                     personRoleVisibility == Visibility.Visible ||
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
        private async void ReloadImage(BaseItemDto item)
        {
            if (ViewModel.ImageDisplayWidth.Equals(0) || ViewModel.ImageDisplayWidth.Equals(0))
            {
                return;
            }

            MainGrid.Height = ViewModel.ImageDisplayHeight;

            MainGrid.Width = ViewModel.ImageDisplayWidth;

            await SetImageSource(item);
        }

        private static readonly Task TrueTaskResult = Task.FromResult(true);

        private Task SetImageSource(BaseItemDto item)
        {
            if (string.Equals(ViewModel.ViewType, ViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase))
            {
                if (item.HasThumb)
                {
                    ItemImage.Stretch = Stretch.UniformToFill;

                    var url = ViewModel.GetImageUrl(ImageType.Thumb);

                    return SetImage(url);
                }
                if (item.BackdropCount > 0)
                {
                    ItemImage.Stretch = Stretch.UniformToFill;

                    var url = ViewModel.GetImageUrl(ImageType.Backdrop);

                    return SetImage(url);
                }
            }

            if (item.HasPrimaryImage)
            {
                ItemImage.Stretch = Stretch.Uniform;

                var url = ViewModel.GetImageUrl(ImageType.Primary);

                return SetImage(url);
            }

            SetDefaultImage();

            return TrueTaskResult;
        }

        private async Task SetImage(string url)
        {
            MainGrid.Background = null;

            try
            {
                ItemImage.Source = await ViewModel.ImageManager.GetRemoteBitmapAsync(url);
            }
            catch (HttpException)
            {
                SetDefaultImage();
            }
        }

        /// <summary>
        /// Sets the default image.
        /// </summary>
        private void SetDefaultImage()
        {
            if (Item.IsAudio || Item.IsType("MusicAlbum") || Item.IsType("MusicArtist"))
            {
                var imageUri = new Uri("../Resources/Images/AudioDefault.png", UriKind.Relative);

                ItemImage.Source = ViewModel.ImageManager.GetBitmapImage(imageUri);
            }
            else
            {
                var imageUri = new Uri("../Resources/Images/VideoDefault.png", UriKind.Relative);

                ItemImage.Source = ViewModel.ImageManager.GetBitmapImage(imageUri);
            }
        }
    }
}
