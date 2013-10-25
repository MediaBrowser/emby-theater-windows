using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for ItemRating.xaml
    /// </summary>
    public partial class ItemRating : UserControl
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
        public ItemRating()
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
                UpdateItemInfo(Item.Item);
            }
        }

        /// <summary>
        /// Updates the item info.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateItemInfo(BaseItemDto item)
        {
            UpdateCommunityRating(item);
        }

        /// <summary>
        /// Updates the community rating.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateCommunityRating(BaseItemDto item)
        {
            if (!item.CommunityRating.HasValue)
            {
                return;
            }

            var images = new[] { ImgCommunityRating1, ImgCommunityRating2, ImgCommunityRating3, ImgCommunityRating4, ImgCommunityRating5 };

            var rating = item.CommunityRating.Value;

            for (var i = 0; i < 5; i++)
            {
                var img = images[i];

                var starValue = (i + 1) * 2;

                if (rating < starValue - 2)
                {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageEmpty");
                }
                else if (rating < starValue)
                {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageHalf");
                }
                else
                {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageFull");
                }
            }
        }
    }
}
