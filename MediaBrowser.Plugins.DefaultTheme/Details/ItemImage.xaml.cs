using MediaBrowser.Model.Dto;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for ItemLogo.xaml
    /// </summary>
    public partial class ItemImage : UserControl
    {
        public int? DownloadWidth { get; set; }
        public int? DownloadHeight { get; set; }
        public int? DownloadMaxWidth { get; set; }
        public int? DownloadMaxHeight { get; set; }

        public string ImageType { get; set; }

        public Thickness ImageMargin { get; set; }

        /// <summary>
        /// The _item
        /// </summary>
        private ItemViewModel _item;

        private ItemViewModel Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                OnItemChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemInfoFooter" /> class.
        /// </summary>
        public ItemImage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContextChanged += ItemInfoFooter_DataContextChanged;

            Item = DataContext as ItemViewModel;
        }

        void ItemInfoFooter_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Item = DataContext as ItemViewModel;
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected void OnItemChanged()
        {
            if (Item != null)
            {
                UpdateImage(Item);
            }
        }

        private async void UpdateImage(ItemViewModel item)
        {
            var options = GetImageOptions();

            if (string.Equals(ImageType, "EpisodeThumb"))
            {
                if (item.Item.HasPrimaryImage)
                {
                    try
                    {
                        options.ImageType = Model.Entities.ImageType.Primary;

                        var bitmap = await item.GetBitmapImageAsync(options, CancellationToken.None);

                        SetImage(bitmap);

                        return;
                    }
                    catch
                    {
                        // Logged at lower levels
                    }
                }

                if (item.Item.ParentThumbImageTag.HasValue)
                {
                    try
                    {
                        options.ImageType = Model.Entities.ImageType.Thumb;

                        var bitmap = await item.GetBitmapImageAsync(options, CancellationToken.None);

                        SetImage(bitmap);

                        return;
                    }
                    catch
                    {
                        // Logged at lower levels
                    }
                }
            }

            else if (string.Equals(ImageType, "Art"))
            {
                if (item.Item.HasArtImage || item.Item.ParentArtImageTag.HasValue)
                {
                    try
                    {
                        options.ImageType = Model.Entities.ImageType.Art;

                        var bitmap = await item.GetBitmapImageAsync(options, CancellationToken.None);

                        SetImage(bitmap);

                        return;
                    }
                    catch
                    {
                        // Logged at lower levels
                    }
                }
            }
            
            else if (item.Item.HasPrimaryImage)
            {
                try
                {
                    options.ImageType = Model.Entities.ImageType.Primary;

                    var bitmap = await item.GetBitmapImageAsync(options, CancellationToken.None);

                    SetImage(bitmap);

                    return;
                }
                catch
                {
                    // Logged at lower levels
                }
            }
            
            Img.Visibility = Visibility.Collapsed;
        }

        private void SetImage(BitmapImage img)
        {
            Img.Source = img;
            Img.Visibility = Visibility.Visible;
            Img.Margin = ImageMargin;
        }

        private ImageOptions GetImageOptions()
        {
            return new ImageOptions
            {
                Width = DownloadWidth,
                Height = DownloadHeight,
                MaxWidth = DownloadMaxWidth,
                MaxHeight = DownloadMaxHeight

            };
        }
    }
}
