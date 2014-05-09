using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for ItemLogo.xaml
    /// </summary>
    public partial class ItemLogo : UserControl
    {
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
        public ItemLogo()
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
                UpdateLogo(Item);
            }
        }

        private async void UpdateLogo(ItemViewModel item)
        {
            if (item.Item.HasLogo || item.Item.ParentLogoImageTag != null)
            {
                try
                {
                    var bitmap = await item.GetBitmapImageAsync(new ImageOptions
                    {
                        ImageType = ImageType.Logo

                    }, CancellationToken.None);

                    ImgLogo.Source = bitmap;
                    ImgLogo.Visibility = Visibility.Visible;

                    return;
                }
                catch
                {
                    // Logged at lower levels
                }
            }

            ImgLogo.Visibility = Visibility.Collapsed;
        }
    }
}
