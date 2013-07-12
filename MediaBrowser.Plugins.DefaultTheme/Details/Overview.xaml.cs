using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        private readonly BaseItemDto _item;
        private readonly IImageManager _imageManager;

        private readonly IApiClient _apiClient;
        private readonly IScrollInfo _scrollPanel;

        private readonly IPlaybackManager _playbackManager;
        private readonly IPresentationManager _presentation;

        public Overview(BaseItemDto item, IImageManager imageManager, IApiClient apiClient, IScrollInfo scrollPanel, IPlaybackManager playbackManager, IPresentationManager presentation)
        {
            _item = item;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _scrollPanel = scrollPanel;
            _playbackManager = playbackManager;
            _presentation = presentation;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ReloadItem();

            BtnPlay.GotKeyboardFocus += BtnPlay_GotKeyboardFocus;
            BtnResume.GotKeyboardFocus += BtnPlay_GotKeyboardFocus;
            BtnDislike.GotKeyboardFocus += BtnPlay_GotKeyboardFocus;
            BtnPlayTrailer.GotKeyboardFocus += BtnPlay_GotKeyboardFocus;

            BtnPlay.Click += BtnPlay_Click;
            BtnResume.Click += BtnResume_Click;
            BtnPlayTrailer.Click += BtnPlayTrailer_Click;

            Loaded += Overview_Loaded;
            Unloaded += Overview_Unloaded;

        }

        private void ReloadItem()
        {
            BtnPlayTrailer.Visibility = _item.LocalTrailerCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            ReloadImage();
            ReloadDetails();
            ReloadUserDataIcons();
        }

        async void BtnPlayTrailer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var trailers = await _apiClient.GetLocalTrailersAsync(_apiClient.CurrentUserId, _item.Id);

                await _playbackManager.Play(new PlayOptions(trailers.First()));
            }
            catch (HttpException)
            {
                _presentation.ShowDefaultErrorMessage();
            }
        }

        async void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            await _playbackManager.Play(new PlayOptions(_item)
            {
                StartPositionTicks = _item.UserData.PlaybackPositionTicks
            });
        }

        async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            await _playbackManager.Play(new PlayOptions(_item));
        }

        void BtnPlay_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _scrollPanel.SetHorizontalOffset(0);
        }

        void Overview_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Overview_Loaded;
        }

        void Overview_Loaded(object sender, RoutedEventArgs e)
        {
            BtnPlay.Focus();
        }

        private void ReloadDetails()
        {
            SetTitle();

            TxtOverview.Text = _item.Overview ?? string.Empty;

            var directors = _item.People.Where(i => string.Equals(i.Type, PersonType.Director, StringComparison.OrdinalIgnoreCase)).ToList();

            if (directors.Count > 0)
            {
                LblDirector.Visibility = TxtDirector.Visibility = Visibility.Visible;

                TxtDirector.Text = string.Join("  •  ", directors.Select(i => i.Name).ToArray());
            }
            else
            {
                LblDirector.Visibility = TxtDirector.Visibility = Visibility.Collapsed;
            }

            if (_item.Genres.Count > 0)
            {
                LblGenre.Visibility = TxtGenre.Visibility = Visibility.Visible;

                TxtGenre.Text = string.Join("  •  ", _item.Genres.ToArray());
            }
            else
            {
                LblGenre.Visibility = TxtGenre.Visibility = Visibility.Collapsed;
            }

            if (_item.Studios.Length > 0)
            {
                LblStudio.Visibility = TxtStudio.Visibility = Visibility.Visible;

                TxtStudio.Text = string.Join("  •  ", _item.Studios.Select(i => i.Name).ToArray());
            }
            else
            {
                LblStudio.Visibility = TxtStudio.Visibility = Visibility.Collapsed;
            }

            if (_item.PremiereDate.HasValue)
            {
                LblPremiereDate.Visibility = TxtPremiereDate.Visibility = Visibility.Visible;

                TxtPremiereDate.Text = _item.PremiereDate.Value.ToLocalTime().ToShortDateString();

                LblPremiereDate.Text = _item.PremiereDate.Value > DateTime.UtcNow ? "premieres" : "premiered";
            }
            else
            {
                LblPremiereDate.Visibility = TxtPremiereDate.Visibility = Visibility.Collapsed;
            }

            if (_item.Players.HasValue)
            {
                LblPlayers.Visibility = TxtPlayers.Visibility = Visibility.Visible;

                TxtPlayers.Text = _item.Players.Value.ToString();
            }
            else
            {
                LblPlayers.Visibility = TxtPlayers.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(_item.GameSystem))
            {
                LblGameSystem.Visibility = TxtGameSystem.Visibility = Visibility.Visible;

                TxtGameSystem.Text = _item.GameSystem;
            }
            else
            {
                LblGameSystem.Visibility = TxtGameSystem.Visibility = Visibility.Collapsed;
            }
        }

        private void SetTitle()
        {
            if (_item.Taglines.Count > 0)
            {
                TxtTagline.Text = _item.Taglines[0];
            }
            else if (_item.IsType("Episode") || _item.IsType("Audio"))
            {
                TxtTagline.Text = _item.Name;
            }
            else
            {
                TxtTagline.Text = string.Empty;
            }
        }

        private async void ReloadImage()
        {
            if (_item.HasPrimaryImage)
            {
                try
                {
                    PrimaryImage.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetImageUrl(_item, new ImageOptions
                    {
                        ImageType = ImageType.Primary
                    }));
                }
                catch (HttpException)
                {
                    // Already logged at lower levels
                }
            }

            SetDefaultImage();
        }

        private void SetDefaultImage()
        {

        }

        private void ReloadUserDataIcons()
        {
            var userData = _item.UserData;

            if (userData.Played)
            {
                ImgPlayed.Visibility = Visibility.Visible;
            }
            else
            {
                ImgPlayed.Visibility = Visibility.Collapsed;

                if (_item.RecentlyAddedItemCount > 0)
                {
                    ImgNew.Visibility = Visibility.Visible;
                    TxtNew.Text = _item.RecentlyAddedItemCount + " NEW";
                }
                else if (_item.DateCreated.HasValue && (DateTime.UtcNow - _item.DateCreated.Value).TotalDays < 14)
                {
                    ImgNew.Visibility = Visibility.Visible;
                    TxtNew.Text = "NEW";
                }
                else
                {
                    ImgNew.Visibility = Visibility.Collapsed;
                }
            }

            if (userData.PlaybackPositionTicks > 0 && _item.RunTimeTicks.HasValue)
            {
                BtnResume.Visibility = Visibility.Visible;
            }
            else
            {
                BtnResume.Visibility = Visibility.Collapsed;
            }

            //if (userData.PlaybackPositionTicks > 0 && _item.RunTimeTicks.HasValue)
            //{
            //    TxtResume.Text = "Resume " + GetTimeString(userData.PlaybackPositionTicks) + " / " +
            //                     GetTimeString(_item.RunTimeTicks.Value);
            //}
        }

        private string GetTimeString(long ticks)
        {
            var time = TimeSpan.FromTicks(ticks);

            if (time.Hours == 0)
            {
                return time.ToString(@"m\:ss");
            }

            return time.ToString(@"h\:mm\:ss");
        }
    }
}
