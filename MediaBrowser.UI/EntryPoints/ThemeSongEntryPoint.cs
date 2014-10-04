using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.UI.EntryPoints
{
    class ThemeSongEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playback;
        private readonly ISessionManager _session;
        private readonly IConnectionManager _connectionManager;

        private string _currentPlayingOwnerId;

        private string _lastPlayedOwnerId;

        public ThemeSongEntryPoint(INavigationService nav, IPlaybackManager playback, ISessionManager session, IConnectionManager connectionManager)
        {
            _nav = nav;
            _playback = playback;
            _session = session;
            _connectionManager = connectionManager;
        }

        public void Run()
        {
            _nav.Navigated += _nav_Navigated;
            _playback.PlaybackCompleted += _playback_PlaybackCompleted;
        }

        void _playback_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            _currentPlayingOwnerId = null;
        }

        async void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            // If something is already playing, and it's not a theme song, leave it alone
            if (string.IsNullOrEmpty(_currentPlayingOwnerId) && _playback.MediaPlayers.Any(i => i.PlayState != PlayState.Idle))
            {
                return;
            }

            var itemPage = e.NewPage as ISupportsItemThemeMedia;

            if (itemPage == null || _session.CurrentUser == null || string.IsNullOrEmpty(itemPage.ThemeMediaItemId))
            {
                if (!string.IsNullOrEmpty(_currentPlayingOwnerId) && !(e.NewPage is ISupportsThemeMedia))
                {
                    _playback.StopAllPlayback();
                }
                return;
            }

            var itemId = itemPage.ThemeMediaItemId;

            var apiClient = _session.ActiveApiClient;

            var themeMediaResult = await GetThemeMedia(apiClient, itemId).ConfigureAwait(false);

            if (string.Equals(_currentPlayingOwnerId, themeMediaResult.OwnerId))
            {
                return;
            }

            // Don't replay the same one over and over
            if (string.Equals(_lastPlayedOwnerId, themeMediaResult.OwnerId))
            {
                return;
            }

            _lastPlayedOwnerId = null;

            if (themeMediaResult.Items.Length > 0)
            {
                await Play(GetItemsToPlay(themeMediaResult)).ConfigureAwait(false);

                _currentPlayingOwnerId = themeMediaResult.OwnerId;
                _lastPlayedOwnerId = themeMediaResult.OwnerId;
            }
            else if (!string.IsNullOrEmpty(_currentPlayingOwnerId))
            {
                _playback.StopAllPlayback();
            }
        }

        private IEnumerable<BaseItemDto> GetItemsToPlay(ThemeMediaResult result)
        {
            var items = result.Items.OrderBy(j => Guid.NewGuid()).ToList();

            var i = 0;

            while (i < 3)
            {
                items.AddRange(result.Items);
                i++;
            }

            return items;
        }

        private Task Play(IEnumerable<BaseItemDto> items)
        {
            return _playback.Play(new PlayOptions
            {
                GoFullScreen = false,
                Items = items.ToList()
            });
        }

        private async Task<ThemeMediaResult> GetThemeMedia(IApiClient apiClient, string itemId)
        {
            var themeMediaResult = await apiClient.GetAllThemeMediaAsync(_session.CurrentUser.Id, itemId, true, CancellationToken.None).ConfigureAwait(false);

            if (themeMediaResult.ThemeVideosResult.Items.Length > 0)
            {
                return themeMediaResult.ThemeVideosResult;
            }

            if (themeMediaResult.ThemeSongsResult.Items.Length > 0)
            {
                return themeMediaResult.ThemeSongsResult;
            }
            
            return themeMediaResult.SoundtrackSongsResult;
        }

        public void Dispose()
        {
            _nav.Navigated -= _nav_Navigated;
            _playback.PlaybackCompleted -= _playback_PlaybackCompleted;
        }
    }
}
