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
    public partial class ItemInfoFooter : BaseDetailsControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemInfoFooter" /> class.
        /// </summary>
        public ItemInfoFooter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected override void OnItemChanged()
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

            PnlPersonalRating.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Updates the official rating.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateOfficialRating(BaseItemDto item)
        {
            if (!string.IsNullOrEmpty(item.OfficialRating))
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
                TxtDate.Text = item.ProductionYear.Value.ToString();
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

                var starValue = (i + 1)*2;

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
