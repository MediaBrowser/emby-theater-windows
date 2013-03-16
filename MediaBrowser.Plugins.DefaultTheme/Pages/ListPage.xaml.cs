using System.Linq;
using System.Windows.Controls;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Plugins.DefaultTheme.DisplayPreferences;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.UI;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.Pages;
using System;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for ListPage.xaml
    /// </summary>
    public partial class ListPage : BaseListPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListPage" /> class.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        public ListPage(string itemId)
            : base(itemId)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Subclasses must provide the list box that holds the items
        /// </summary>
        /// <value>The items list.</value>
        protected override ExtendedListBox ItemsList
        {
            get
            {
                return lstItems;
            }
        }

        /// <summary>
        /// If the page is using it's own image type and not honoring the DisplayPreferences setting, it should return it here
        /// </summary>
        /// <value>The type of the fixed image.</value>
        protected override ImageType? FixedImageType
        {
            get { return ImageType.Primary; }
        }

        /// <summary>
        /// Called when [loaded].
        /// </summary>
        protected override async void OnLoaded()
        {
            base.OnLoaded();

            if (Folder != null)
            {
                ShowViewButton();

                await AppResources.Instance.SetPageTitle(Folder);
            }
            else
            {
                HideViewButton();
            }
        }

        /// <summary>
        /// Called when [unloaded].
        /// </summary>
        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            HideViewButton();
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name.Equals("CurrentItemIndex", StringComparison.OrdinalIgnoreCase))
            {
                UpdateCurrentItemIndex();
            }
        }

        /// <summary>
        /// Updates the index of the current item.
        /// </summary>
        private void UpdateCurrentItemIndex()
        {
            var index = CurrentItemIndex;

            currentItemIndex.Visibility = index == -1 ? Visibility.Collapsed : Visibility.Visible;
            currentItemIndex.Text = (CurrentItemIndex + 1).ToString();

            currentItemIndexDivider.Visibility = index == -1 ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Gets called anytime the Folder gets refreshed
        /// </summary>
        protected override async void OnFolderChanged()
        {
            base.OnFolderChanged();

            var pageTitleTask = AppResources.Instance.SetPageTitle(Folder);

            ShowViewButton();

            if (Folder.IsType("Season"))
            {
                TxtName.Visibility = Visibility.Visible;
                TxtName.Text = Folder.Name;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
            }

            await pageTitleTask;
        }

        /// <summary>
        /// Shows the view button.
        /// </summary>
        private void ShowViewButton()
        {
            var viewButton = AppResources.Instance.ViewButton;
            viewButton.Visibility = Visibility.Visible;
            viewButton.Click -= ViewButton_Click;
            viewButton.Click += ViewButton_Click;
        }

        /// <summary>
        /// Hides the view button.
        /// </summary>
        private void HideViewButton()
        {
            var viewButton = AppResources.Instance.ViewButton;
            viewButton.Visibility = Visibility.Collapsed;
            viewButton.Click -= ViewButton_Click;
        }

        /// <summary>
        /// Handles the Click event of the ViewButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = new DisplayPreferencesMenu
            {
                FolderId = Folder.Id,
                MainPage = this
            };

            menu.ShowModal(this.GetWindow());

            try
            {
                await App.Instance.ApiClient.UpdateDisplayPreferencesAsync(App.Instance.CurrentUser.Id, Folder.Id, DisplayPreferences);
            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Handles current item selection changes
        /// </summary>
        protected override void OnCurrentItemChanged()
        {
            base.OnCurrentItemChanged();

            if (CurrentItem == null)
            {
                PnlItemInfo.Visibility = Visibility.Hidden;
            }
            else
            {
                PnlItemInfo.Visibility = Visibility.Visible;
                UpdateItemInfo(CurrentItem);
            }
        }

        private void UpdateItemInfo(BaseItemDto item)
        {
            UpdateCommunityRating(item);
            UpdateOfficialRating(item);
            UpdateRuntime(item);
            UdpateDate(item);
            UpdateVideoInfo(item);
            UpdateAudioCodec(item);
            UpdateAudioInfo(item);
        }
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
        private void UpdateCommunityRating(BaseItemDto item)
        {
            if (!item.CommunityRating.HasValue)
            {
                PnlCommunityRating.Visibility = Visibility.Hidden;
                return;
            }

            PnlCommunityRating.Visibility = Visibility.Visible;

            var images = new[] { ImgCommunityRating1, ImgCommunityRating2, ImgCommunityRating3, ImgCommunityRating4, ImgCommunityRating5 };

            var rating = item.CommunityRating.Value / 2;

            for (var i = 0; i < 5; i++)
            {
                var img = images[i];

                if (rating < i - 1)
                {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageEmpty");
                }
                else if (rating < i)
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

            return width == 0 || height == 0 ? string.Empty : width + "x" + height;
        }
    }
}
