using System.Globalization;
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
        private BaseItemDto Item
        {
            get { return ViewModel.Item; }
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

        private static readonly CultureInfo UsCulture = new CultureInfo("en-US");
        
        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            ReloadImage();

            var item = Item;
            var parentName = item.SeriesName;

            if (string.IsNullOrEmpty(parentName))
            {
                parentName = item.Album;
            }

            if (string.IsNullOrEmpty(parentName))
            {
                TxtParentName.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtParentName.Text = parentName;
            }

            var name = Item.Name;

            if (Item.IsType("Episode") && item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue)
            {
                var displayIndexNumber = item.IndexNumber < 10
                                             ? "0" + item.IndexNumber.Value.ToString(UsCulture)
                                             : item.IndexNumber.Value.ToString(UsCulture);

                var number = item.ParentIndexNumber + "x" + displayIndexNumber;

                if (item.IndexNumberEnd.HasValue)
                {
                    displayIndexNumber = item.IndexNumberEnd < 10
                                             ? "0" + item.IndexNumberEnd.Value.ToString(UsCulture)
                                             : item.IndexNumberEnd.Value.ToString(UsCulture);

                    number += "-x" + displayIndexNumber;
                }

                name = number + " - " + name;
            }
            else
            {
                if (item.IndexNumber.HasValue && !item.IsType("Season")) {
                    name = item.IndexNumber.Value.ToString(UsCulture) + " - " + name;
                }
                if (item.ParentIndexNumber != null && !item.IsType("Episode"))
                {
                    name = item.ParentIndexNumber.Value.ToString(UsCulture) + "." + name;
                }
            }

            TxtName.Text = name;
        }

        /// <summary>
        /// Reloads the image.
        /// </summary>
        private void ReloadImage()
        {
            if (Item.HasPrimaryImage)
            {
                var url = ViewModel.ApiClient.GetImageUrl(Item, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    Height = 225
                });

                SetImage(url);
            }
            else if (Item.BackdropCount > 0)
            {
                var url = ViewModel.ApiClient.GetImageUrl(Item, new ImageOptions
                {
                    ImageType = ImageType.Backdrop,
                    Height = 225,
                    Width = 400
                });

                SetImage(url);
            }
            else if (Item.HasThumb)
            {
                var url = ViewModel.ApiClient.GetImageUrl(Item, new ImageOptions
                {
                    ImageType = ImageType.Thumb,
                    Height = 225,
                    Width = 400
                });

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
                image.Source = await ViewModel.ImageManager.GetRemoteBitmapAsync(url);
            }
            catch (HttpException)
            {
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            var imageUri = new Uri("../Resources/Images/VideoDefault.png", UriKind.Relative);
            image.Source = ViewModel.ImageManager.GetBitmapImage(imageUri);
        }
    }
}
