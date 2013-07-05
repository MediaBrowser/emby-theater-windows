using System.Windows.Media;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for BaseItemTile.xaml
    /// </summary>
    public partial class HomePageTile : UserControl
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
        public BaseItemDto Item
        {
            get
            {
                var vm = ViewModel;

                if (vm == null)
                {
                    return null;
                }
                return vm.Item;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePageTile" /> class.
        /// </summary>
        public HomePageTile()
        {
            InitializeComponent();

            DataContextChanged += BaseItemTile_DataContextChanged;
        }

        /// <summary>
        /// Handles the DataContextChanged event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnItemChanged();
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            var item = Item;

            if (item == null)
            {
                return;
            }

            MainElement.Width = ViewModel.ImageDisplayWidth;
            MainElement.Height = ViewModel.ImageDisplayHeight;

            ReloadImage(item);

            if (item.CanResume && item.RunTimeTicks.HasValue)
            {
                Progress.Visibility = Visibility.Visible;
                Progress.Maximum = item.RunTimeTicks.Value;

                Progress.Value = item.UserData.PlaybackPositionTicks;
            }
            else
            {
                Progress.Visibility = Visibility.Collapsed;
            }

            TxtName.Text = MultiItemTile.GetDisplayName(item);
        }

        /// <summary>
        /// Reloads the image.
        /// </summary>
        private void ReloadImage(BaseItemDto item)
        {
            if (ViewModel.ImageType == ImageType.Primary && item.HasPrimaryImage)
            {
                var url = ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    Height = Convert.ToInt32(ViewModel.ImageDisplayHeight)
                });

                Image.Stretch = Stretch.Uniform;
                SetImage(url);
            }
            else if (item.BackdropCount > 0)
            {
                var url = ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Backdrop,
                    Height = Convert.ToInt32(ViewModel.ImageDisplayHeight),
                    Width = Convert.ToInt32(ViewModel.ImageDisplayWidth)
                });

                Image.Stretch = Stretch.UniformToFill;
                SetImage(url);
            }
            else if (item.HasThumb)
            {
                var url = ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Thumb,
                    Height = Convert.ToInt32(ViewModel.ImageDisplayHeight),
                    Width = Convert.ToInt32(ViewModel.ImageDisplayWidth)
                });

                Image.Stretch = Stretch.UniformToFill;
                SetImage(url);
            }
            else if (item.HasPrimaryImage)
            {
                var url = ViewModel.ApiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    Height = Convert.ToInt32(ViewModel.ImageDisplayHeight)
                });

                Image.Stretch = Stretch.Uniform;
                SetImage(url);
            }
            else
            {
                SetDefaultImage();
            }
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="url">The URL.</param>
        private async void SetImage(string url)
        {
            try
            {
                Image.Source = await ViewModel.ImageManager.GetRemoteBitmapAsync(url);
            }
            catch (HttpException)
            {
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            var imageUri = new Uri("../Resources/Images/VideoDefault.png", UriKind.Relative);
            Image.Source = ViewModel.ImageManager.GetBitmapImage(imageUri);
        }
    }
}
