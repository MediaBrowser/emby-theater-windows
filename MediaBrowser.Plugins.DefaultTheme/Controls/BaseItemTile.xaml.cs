using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.UI;
using MediaBrowser.UI.ViewModels;
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

            var nameVisibility = !string.Equals(ViewModel.ViewType, ViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase) && Item.HasPrimaryImage && !Item.IsType("Episode") && !Item.IsType("Audio") ? Visibility.Collapsed : Visibility.Visible;

            if (item.IsType("Person") || item.IsType("IndexFolder"))
            {
                nameVisibility = Visibility.Visible;
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

            var playedVisibility = Visibility.Collapsed;
            if (!item.CanResume && item.UserData != null && item.UserData.Played)
            {
                playedVisibility = Visibility.Visible;
            }
            ImgPlayed.Visibility = playedVisibility;

            var newVisibility = Visibility.Collapsed;
            if (item.DateCreated.HasValue && (DateTime.UtcNow - item.DateCreated.Value).TotalDays < 14)
            {
                newVisibility = Visibility.Visible;
            }
            ImgNew.Visibility = newVisibility;
        

            OverlayGrid.Visibility = nameVisibility == Visibility.Visible ||
                                     playedVisibility == Visibility.Visible ||
                                     newVisibility == Visibility.Visible ||
                                     progressBarVisibility == Visibility.Visible
                                         ? Visibility.Visible
                                         : Visibility.Collapsed;
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
            if (ViewModel.ImageWidth.Equals(0) || ViewModel.ImageHeight.Equals(0))
            {
                return;
            }

            MainGrid.Height = ViewModel.ImageHeight;

            MainGrid.Width = ViewModel.ImageWidth;

            await SetImageSource(item);
        }

        private static readonly Task TrueTaskResult = Task.FromResult(true);

        private Task SetImageSource(BaseItemDto item)
        {
            if (string.Equals(ViewModel.ViewType, ViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase))
            {
                if (item.HasThumb)
                {
                    var url = ViewModel.GetImageUrl(ImageType.Thumb);

                    return SetImage(url);
                }
                if (item.BackdropCount > 0)
                {
                    var url = ViewModel.GetImageUrl(ImageType.Backdrop);

                    return SetImage(url);
                }
            }
            
            if (item.HasPrimaryImage)
            {
                var url = ViewModel.GetImageUrl(ImageType.Primary);

                return SetImage(url);
            }

            SetDefaultImage();

            return TrueTaskResult;
        }

        private async Task SetImage(string url)
        {
            ImageBorder.Background = null;

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
