using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using MediaBrowser.Theater.DefaultTheme.ItemList;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemOverviewViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly BaseItemDto _item;
        private bool _isWatched;
        private bool _isLiked;
        private bool _isDisliked;
        private bool _isFavorited;

        public ItemArtworkViewModel PosterArtwork { get; set; }
        public ItemArtworkViewModel BackgroundArtwork { get; set; }
        public ItemInfoViewModel Info { get; set; }

        public ICommand PlayCommand { get; set; }
        public ICommand EnqueueCommand { get; set; }
        public bool CanPlay { get; set; }

        public ICommand PlayAllCommand { get; set; }
        public ICommand EnqueueAllCommand { get; set; }
        public bool CanPlayAll { get; set; }

        public ICommand ResumeCommand { get; set; }

        public ICommand BrowseAllCommand { get; set; }

        public ICommand PrimaryCommand { get; set; }
        public ICommand SecondaryCommand { get; set; }
        public ICommand ToggleFavoriteCommand { get; set; }
        public ICommand ToggleLikeCommand { get; set; }
        public ICommand ToggleDislikeCommand { get; set; }
        public ICommand ToggleWatchedCommand { get; set; }

        public int SortOrder
        {
            get { return 0; }
        }

        public string Title
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_OverviewHeader".Localize(); }
        }

        public bool ShowInfo
        {
            get { return (!_item.IsFolder && _item.Type != "Person") || !string.IsNullOrEmpty(_item.Overview); }
        }

        public double PosterHeight
        {
            get { return HomeViewModel.TileHeight*3 + HomeViewModel.TileMargin*4; }
        }

        public double DetailsWidth
        {
            get { return HomeViewModel.TileWidth*2 + HomeViewModel.TileMargin*2; }
        }

        public double DetailsHeight
        {
            get { return HomeViewModel.TileHeight * 2 + HomeViewModel.TileMargin * 2; }
        }

        public bool IsWatched
        {
            get { return _isWatched; }
            private set
            {
                if (value.Equals(_isWatched)) {
                    return;
                }
                _isWatched = value;
                OnPropertyChanged();
            }
        }

        public bool IsLiked
        {
            get { return _isLiked; }
            private set
            {
                if (value.Equals(_isLiked)) {
                    return;
                }
                _isLiked = value;
                OnPropertyChanged();
            }
        }

        public bool IsDisliked
        {
            get { return _isDisliked; }
            private set
            {
                if (value.Equals(_isDisliked)) {
                    return;
                }
                _isDisliked = value;
                OnPropertyChanged();
            }
        }

        public bool IsFavorited
        {
            get { return _isFavorited; }
            private set
            {
                if (value.Equals(_isFavorited)) {
                    return;
                }
                _isFavorited = value;
                OnPropertyChanged();
            }
        }

        public PlayButtonViewModel PlayButton { get; private set; }

        public ItemOverviewViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager, ISessionManager sessionManager, INavigator navigator)
        {
            _item = item;

            if (item.UserData != null) {
                IsWatched = item.UserData.Played;
                IsLiked = item.UserData.Likes ?? false;
                IsDisliked = !(item.UserData.Likes ?? true);
                IsFavorited = item.UserData.IsFavorite;
            }

            Info = new ItemInfoViewModel(item) {
                ShowDisplayName = true,
                ShowUserRatings = false,
                ShowParentText = item.IsType("Season") || item.IsType("Episode") || item.IsType("Album") || item.IsType("Track")
            };

            if (item.ImageTags.ContainsKey(ImageType.Primary)) {
                PosterArtwork = new ItemArtworkViewModel(item, connectionManager, imageManager) {
                    DesiredImageHeight = PosterHeight,
                    PreferredImageTypes = new[] { ImageType.Primary }
                };

                PosterArtwork.PropertyChanged += (s, e) => {
                    if (e.PropertyName == "Size") {
                        OnPropertyChanged("Size");
                    }
                };
            }

            BackgroundArtwork = new ItemArtworkViewModel(item, connectionManager, imageManager) {
                DesiredImageWidth = DetailsWidth,
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Art, ImageType.Banner, ImageType.Screenshot, ImageType.Primary }
            };

            PlayCommand = new RelayCommand(o => playbackManager.Play(item));
            ResumeCommand = new RelayCommand(o => playbackManager.Play(Media.Resume(item)));
            PlayAllCommand = new RelayCommand(async o => {
                var items = await ItemChildren.Get(connectionManager, sessionManager, item, new ChildrenQueryParams {
                    Recursive = true,
                    IncludeItemTypes = new[] { "Movie", "Episode", "Audio" },
                    SortOrder = MediaBrowser.Model.Entities.SortOrder.Ascending,
                    SortBy = new[] { "SortName" }
                });
                
                if (items.Items.Length > 0) {
                    await playbackManager.Play(items.Items.Select(i => (Media)i));
                }
            });

            BrowseAllCommand = new RelayCommand(o => navigator.Navigate(Go.To.ItemList(new ItemListParameters {
                Items = ItemChildren.Get(connectionManager, sessionManager, item, new ChildrenQueryParams { ExpandSingleItems = true }),
                Title = item.Name
            })));
            
            PlayButton = new PlayButtonViewModel(item, playbackManager, connectionManager, imageManager, sessionManager, item.BackdropImageTags.Count > 1 ? 1 : (int?)null);

            var api = connectionManager.GetApiClient(item);
            
            ToggleWatchedCommand = new RelayCommand(o => {
                if (IsWatched) {
                    api.MarkPlayedAsync(item.Id, sessionManager.CurrentUser.Id, null);
                } else {
                    api.MarkUnplayedAsync(item.Id, sessionManager.CurrentUser.Id);
                }

                IsWatched = !IsWatched;
            });

            ToggleLikeCommand = new RelayCommand(o => {
                if (IsLiked || IsDisliked) {
                    api.ClearUserItemRatingAsync(item.Id, sessionManager.CurrentUser.Id);
                    IsLiked = false;
                    IsDisliked = false;
                } else if (!IsLiked) {
                    api.UpdateUserItemRatingAsync(item.Id, sessionManager.CurrentUser.Id, true);
                    IsLiked = true;
                    IsDisliked = false;
                }
            });

            ToggleDislikeCommand = new RelayCommand(o => {
                if (IsLiked || IsDisliked) {
                    api.ClearUserItemRatingAsync(item.Id, sessionManager.CurrentUser.Id);
                    IsLiked = false;
                    IsDisliked = false;
                } else if (!IsDisliked) {
                    api.UpdateUserItemRatingAsync(item.Id, sessionManager.CurrentUser.Id, false);
                    IsLiked = false;
                    IsDisliked = true;
                }
            });

            ToggleFavoriteCommand = new RelayCommand(o => {
                api.UpdateFavoriteStatusAsync(item.Id, sessionManager.CurrentUser.Id, !IsFavorited);
                IsFavorited = !IsFavorited;
            });
        }

        public Size Size
        {
            get
            {
//                if (ShowInfo)
//                    return new Size(800 + 20 + 250, 700);

                var artWidth = Math.Min(1200, PosterArtwork != null ? PosterArtwork.ActualWidth : 0);
//                return new Size(artWidth + 20 + 250, 700);
                return new Size(artWidth + DetailsWidth + 4, PosterHeight);
            }
        }
    }

    public class ItemOverviewSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly ISessionManager _sessionManager;
        private readonly INavigator _navigator;

        public ItemOverviewSectionGenerator(IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager, ISessionManager sessionManager, INavigator navigator)
        {
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _playbackManager = playbackManager;
            _sessionManager = sessionManager;
            _navigator = navigator;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item != null;
        }

        public Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            IItemDetailSection section = new ItemOverviewViewModel(item, _connectionManager, _imageManager, _playbackManager, _sessionManager, _navigator);
            return Task.FromResult<IEnumerable<IItemDetailSection>>(new[] { section });
        }
    }
}