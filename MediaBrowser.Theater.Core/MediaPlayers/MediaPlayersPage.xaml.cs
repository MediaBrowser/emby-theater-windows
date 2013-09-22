using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MediaBrowser.Theater.Core.MediaPlayers
{
    /// <summary>
    /// Interaction logic for MediaPlayersPage.xaml
    /// </summary>
    public partial class MediaPlayersPage : BasePage
    {
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IPresentationManager _presentation;
        private readonly IApiClient _apiClient;

        public MediaPlayersPage(INavigationService nav, IPlaybackManager playbackManager, ITheaterConfigurationManager config, IPresentationManager presentation, IApiClient apiClient)
        {
            _nav = nav;
            _playbackManager = playbackManager;
            _config = config;
            _presentation = presentation;
            _apiClient = apiClient;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            BtnAddPlayer.Click += BtnAddPlayer_Click;
            LstItems.ItemInvoked += LstItems_ItemInvoked;

            var items = new RangeObservableCollection<MediaPlayerViewModel>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_config.Configuration.MediaPlayers.Select(i => new MediaPlayerViewModel(i)));
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var player = (MediaPlayerViewModel)e.Argument;

            await _nav.Navigate(new ConfigureMediaPlayerPage(player.PlayerConfiguration, _playbackManager, _config, _presentation, _nav, _apiClient));
        }

        async void BtnAddPlayer_Click(object sender, RoutedEventArgs e)
        {
            await _nav.Navigate(new ConfigureMediaPlayerPage(_playbackManager, _config, _presentation, _nav, _apiClient));
        }
    }

    public class MediaPlayerViewModel
    {
        public MediaPlayerViewModel(PlayerConfiguration config)
        {
            PlayerConfiguration = config;
        }

        public string Name
        {
            get { return PlayerConfiguration.PlayerName; }
        }

        public string Attributes
        {
            get
            {
                if (string.Equals(PlayerConfiguration.MediaType, Model.Entities.MediaType.Video, StringComparison.OrdinalIgnoreCase))
                {
                    var attributes = new List<string>();

                    if (PlayerConfiguration.PlayBluray)
                    {
                        attributes.Add("Blu-ray");
                    }

                    if (PlayerConfiguration.PlayDvd)
                    {
                        attributes.Add("Dvd");
                    }

                    if (PlayerConfiguration.Play3DVideo)
                    {
                        attributes.Add("3D");
                    }

                    return string.Join(", ", attributes.ToArray());
                }
                if (string.Equals(PlayerConfiguration.MediaType, Model.Entities.MediaType.Game, StringComparison.OrdinalIgnoreCase))
                {

                }
                return string.Empty;
            }
        }

        public string MediaType
        {
            get
            {
                var attributes = Attributes;

                return !string.IsNullOrEmpty(attributes) ? attributes : PlayerConfiguration.MediaType;
            }
        }

        public string FileExtensions
        {
            get { return string.Join(", ", PlayerConfiguration.FileExtensions.ToArray()); }
        }

        public Visibility FileExtensionsVisibility
        {
            get { return PlayerConfiguration.FileExtensions.Any() ? Visibility.Visible : Visibility.Collapsed; }
        }

        public PlayerConfiguration PlayerConfiguration { get; private set; }
    }
}
