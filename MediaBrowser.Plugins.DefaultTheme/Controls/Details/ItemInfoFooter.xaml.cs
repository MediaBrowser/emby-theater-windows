using System.ComponentModel;
using System.Windows.Controls;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using System;
using System.Linq;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Interaction logic for ItemInfoFooter.xaml
    /// </summary>
    public partial class ItemInfoFooter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool EnableSmallText { get; set; }

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// The _item
        /// </summary>
        private BaseItemDto _item;
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                _item = value;
                OnPropertyChanged("Item");
                OnItemChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemInfoFooter" /> class.
        /// </summary>
        public ItemInfoFooter()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += ItemInfoFooter_Loaded;
        }

        void ItemInfoFooter_Loaded(object sender, RoutedEventArgs e)
        {
            if (EnableSmallText)
            {
                SetSmallText();
            }
        }

        private void SetSmallText()
        {

            TxtAudioChannels.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
            TxtAudioCodec.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
            TxtCriticRating.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
            TxtDate.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
            TxtOfficialRating.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
            TxtResolution.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
            TxtRuntime.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected void OnItemChanged()
        {
            if (Item == null)
            {
                PnlItemInfo.Visibility = Visibility.Hidden;
            }
            else
            {
                PnlItemInfo.Visibility = Visibility.Visible;
                UpdateItemInfo(Item);
            }
        }

        /// <summary>
        /// Updates the item info.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateItemInfo(BaseItemDto item)
        {
            UpdatePersonalRating(item);
            UpdateCommunityRating(item);
            UpdateOfficialRating(item);
            UpdateRuntime(item);
            UdpateDate(item);
            UpdateVideoInfo(item);
            UpdateAudioCodec(item);
            UpdateAudioInfo(item);
            UpdateCriticRating(item);
        }

        private void UpdateCriticRating(BaseItemDto item)
        {
            if (item.CriticRating.HasValue)
            {
                PnlCriticRating.Visibility = Visibility.Visible;

                ImgFresh.Visibility = item.CriticRating.Value >= 60 ? Visibility.Visible : Visibility.Collapsed;
                ImgRotten.Visibility = item.CriticRating.Value < 60 ? Visibility.Visible : Visibility.Collapsed;

                TxtCriticRating.Text = item.CriticRating.Value.ToString() + "%";
            }
            else
            {
                PnlCriticRating.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdatePersonalRating(BaseItemDto item)
        {
            var userdata = item.UserData;

            if (userdata != null)
            {
                if (userdata.Likes.HasValue || userdata.IsFavorite)
                {
                    PnlPersonalRating.Visibility = Visibility.Visible;

                    if (userdata.IsFavorite)
                    {
                        ImgFavorite.Visibility = Visibility.Visible;
                        ImgLike.Visibility = Visibility.Collapsed;
                        ImgDislike.Visibility = Visibility.Collapsed;
                    }

                    else if (userdata.Likes.HasValue && userdata.Likes.Value)
                    {
                        ImgFavorite.Visibility = Visibility.Collapsed;
                        ImgLike.Visibility = Visibility.Visible;
                        ImgDislike.Visibility = Visibility.Collapsed;
                    }
                    else if (userdata.Likes.HasValue && !userdata.Likes.Value)
                    {
                        ImgFavorite.Visibility = Visibility.Collapsed;
                        ImgLike.Visibility = Visibility.Collapsed;
                        ImgDislike.Visibility = Visibility.Visible;
                    }

                    return;
                }
            }

            PnlPersonalRating.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Updates the official rating.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateOfficialRating(BaseItemDto item)
        {
            if (!string.IsNullOrEmpty(item.OfficialRating) && !item.IsType("episode"))
            {
                TxtOfficialRating.Text = item.OfficialRating;
                PnlOfficialRating.Visibility = Visibility.Visible;
                return;
            }

            PnlOfficialRating.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Updates the runtime.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateRuntime(BaseItemDto item)
        {
            if (item.RunTimeTicks.HasValue)
            {
                var minutes = Math.Round(TimeSpan.FromTicks(item.RunTimeTicks.Value).TotalMinutes);

                TxtRuntime.Text = minutes < 2 ? minutes + " min" : minutes + " mins";
                PnlRuntime.Visibility = Visibility.Visible;
                return;
            }

            PnlRuntime.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Udpates the date.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UdpateDate(BaseItemDto item)
        {
            if (item.PremiereDate.HasValue && item.IsType("episode"))
            {
                TxtDate.Text = item.PremiereDate.Value.ToShortDateString();

                PnlDate.Visibility = Visibility.Visible;
                return;
            }
            if (item.ProductionYear.HasValue)
            {
                var text = item.ProductionYear.Value.ToString();

                if (item.EndDate.HasValue && item.EndDate.Value.Year != item.ProductionYear)
                {
                    text += "-" + item.EndDate.Value.Year;
                }
                else if (item.Status.HasValue && item.Status.Value == SeriesStatus.Continuing)
                {
                    text += "-Present";
                }

                TxtDate.Text = text;
                PnlDate.Visibility = Visibility.Visible;
                return;
            }

            PnlDate.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Updates the video info.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateVideoInfo(BaseItemDto item)
        {
            if (item.IsVideo)
            {
                if (item.MediaStreams != null)
                {
                    var videoStream = item.MediaStreams.FirstOrDefault(m => m.Type == MediaStreamType.Video);

                    if (videoStream != null)
                    {
                        PnlVideoInfo.Visibility = Visibility.Visible;
                        TxtResolution.Text = GetResolutionText(videoStream);

                        return;
                    }
                }
            }

            PnlVideoInfo.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Updates the audio info.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateAudioInfo(BaseItemDto item)
        {
            if (item.IsVideo || item.IsAudio)
            {
                if (item.MediaStreams != null)
                {
                    var stream = item.MediaStreams.FirstOrDefault(m => m.Type == MediaStreamType.Audio);

                    if (stream != null)
                    {
                        PnlAudioInfo.Visibility = Visibility.Visible;
                        TxtAudioChannels.Text = (stream.Channels ?? 0) + "ch";

                        return;
                    }
                }
            }

            PnlAudioInfo.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Updates the audio codec.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateAudioCodec(BaseItemDto item)
        {
            if (item.IsAudio)
            {
                if (item.MediaStreams != null)
                {
                    var stream = item.MediaStreams.FirstOrDefault(m => m.Type == MediaStreamType.Audio);

                    if (stream != null)
                    {
                        PnlAudioCodec.Visibility = Visibility.Visible;
                        TxtAudioCodec.Text = stream.Codec ?? "Unknown video codec";

                        return;
                    }
                }
            }

            PnlAudioCodec.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Updates the community rating.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateCommunityRating(BaseItemDto item)
        {
            if (!item.CommunityRating.HasValue)
            {
                PnlCommunityRating.Visibility = Visibility.Collapsed;
                return;
            }

            PnlCommunityRating.Visibility = Visibility.Visible;

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

        /// <summary>
        /// Gets the resolution text.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns>System.String.</returns>
        private string GetResolutionText(MediaStream info)
        {
            var height = info.Height ?? 0;
            var width = info.Width ?? 0;

            if (height == 1080)
            {
                if (string.Equals(info.ScanType, "progressive", StringComparison.OrdinalIgnoreCase))
                {
                    return "1080p";
                }
                if (string.Equals(info.ScanType, "interlaced", StringComparison.OrdinalIgnoreCase))
                {
                    return "1080i";
                }
            }
            if (height == 720)
            {
                if (string.Equals(info.ScanType, "progressive", StringComparison.OrdinalIgnoreCase))
                {
                    return "720p";
                }
                if (string.Equals(info.ScanType, "interlaced", StringComparison.OrdinalIgnoreCase))
                {
                    return "720i";
                }
            }
            if (height == 480)
            {
                if (string.Equals(info.ScanType, "progressive", StringComparison.OrdinalIgnoreCase))
                {
                    return "480p";
                }
                if (string.Equals(info.ScanType, "interlaced", StringComparison.OrdinalIgnoreCase))
                {
                    return "480i";
                }
            }

            return width == 0 || height == 0 ? string.Empty : width + "/" + height;
        }
    }
}
