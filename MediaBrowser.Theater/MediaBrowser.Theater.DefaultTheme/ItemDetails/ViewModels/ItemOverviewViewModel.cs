using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Commands.ItemCommands;
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
        private CroppedBitmap _primaryButtonImage;
        private CroppedBitmap _secondaryButtonImage;
        private CroppedBitmap _toggleFavoriteButtonImage;
        private CroppedBitmap _toggleLikeButtonImage;
        private CroppedBitmap _toggleDislikeButtonImage;
        private CroppedBitmap _toggleWatchedButtonImage;
        private IItemCommand _primaryCommand;
        private IItemCommand _secondaryCommand;

        public ItemArtworkViewModel PosterArtwork { get; set; }
        public ItemArtworkViewModel BackgroundArtwork { get; set; }
        public ItemInfoViewModel Info { get; set; }

        public IItemCommand PrimaryCommand
        {
            get { return _primaryCommand; }
            set
            {
                if (Equals(value, _primaryCommand)) {
                    return;
                }
                _primaryCommand = value;
                OnPropertyChanged();
                OnPropertyChanged("HasPrimaryCommand");
            }
        }

        public IItemCommand SecondaryCommand
        {
            get { return _secondaryCommand; }
            set
            {
                if (Equals(value, _secondaryCommand)) {
                    return;
                }
                _secondaryCommand = value;
                OnPropertyChanged();
                OnPropertyChanged("HasSecondaryCommand");
            }
        }

        public ICommand ToggleFavoriteCommand { get; set; }
        public ICommand ToggleLikeCommand { get; set; }
        public ICommand ToggleDislikeCommand { get; set; }
        public ICommand ToggleWatchedCommand { get; set; }

        public bool HasPrimaryCommand
        {
            get { return PrimaryCommand != null; }
        }

        public bool HasSecondaryCommand
        {
            get { return SecondaryCommand != null; }
        }

        public CroppedBitmap PrimaryButtonImage
        {
            get { return _primaryButtonImage; }
            set
            {
                if (Equals(value, _primaryButtonImage)) {
                    return;
                }
                _primaryButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap SecondaryButtonImage
        {
            get { return _secondaryButtonImage; }
            set
            {
                if (Equals(value, _secondaryButtonImage)) {
                    return;
                }
                _secondaryButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleFavoriteButtonImage
        {
            get { return _toggleFavoriteButtonImage; }
            set
            {
                if (Equals(value, _toggleFavoriteButtonImage)) {
                    return;
                }
                _toggleFavoriteButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleLikeButtonImage
        {
            get { return _toggleLikeButtonImage; }
            set
            {
                if (Equals(value, _toggleLikeButtonImage)) {
                    return;
                }
                _toggleLikeButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleDislikeButtonImage
        {
            get { return _toggleDislikeButtonImage; }
            set
            {
                if (Equals(value, _toggleDislikeButtonImage)) {
                    return;
                }
                _toggleDislikeButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleWatchedButtonImage
        {
            get { return _toggleWatchedButtonImage; }
            set
            {
                if (Equals(value, _toggleWatchedButtonImage)) {
                    return;
                }
                _toggleWatchedButtonImage = value;
                OnPropertyChanged();
            }
        }
        
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

        public ItemOverviewViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager, ISessionManager sessionManager, IItemCommandsManager commands)
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
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Screenshot }
            };
            
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

            SetupButtonImage(item, connectionManager, imageManager);
        }

        private async Task SetupCommands(BaseItemDto item, IItemCommandsManager commandManager)
        {
            var commands = await commandManager.GetCommands(item);
            // pick out play/resume, like, dislike, favorite and watched - bind to static buttons
            // sort rest by sort order
            // pick top 2, set as primary and secondary actions
            // swap empty action slots for images
            // if more than 2 actions, replace watched button with overflow

            var extraActions = commands.Where(c => true).OrderBy(c => c.SortOrder).ToList(); // todo filter item commands
            
            PrimaryCommand = extraActions.FirstOrDefault();
            SecondaryCommand = extraActions.Skip(1).FirstOrDefault();
            
            if (extraActions.Count > 2) {
                
            }
        }

        private async void SetupButtonImage(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager)
        {
            if (item.BackdropCount < 3) {
                return;
            }

            var width = (int) (DetailsWidth/2 - 2);
            var height = (int) HomeViewModel.TileHeight;

            var api = connectionManager.GetApiClient(item);
            var url = api.GetImageUrl(item, new ImageOptions {
                ImageType = ImageType.Backdrop,
                ImageIndex = 2,
                Width = width,
                Height = height
            });

            var bitmap = await imageManager.GetRemoteBitmapAsync(url);
            PrimaryButtonImage = new CroppedBitmap(bitmap, new Int32Rect(0, 0, width/2 - 2, height*2/3 - 2));
            SecondaryButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width/2 + 2, 0, width/2 - 2, height*2/3 - 2));
            ToggleFavoriteButtonImage = new CroppedBitmap(bitmap, new Int32Rect(0, height*2/3 + 2, width/4 - 4, height/3 - 2));
            ToggleLikeButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width/4 + 2, height*2/3 + 2, width/4 - 4, height/3 - 2));
            ToggleDislikeButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width*2/4 + 2, height*2/3 + 2, width/4 - 4, height/3 - 2));
            ToggleWatchedButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width*3/4 + 2, height*2/3 + 2, width/4 - 4, height/3 - 2));
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
            IItemDetailSection section = new ItemOverviewViewModel(item, _connectionManager, _imageManager, _playbackManager, _sessionManager, null);
            return Task.FromResult<IEnumerable<IItemDetailSection>>(new[] { section });
        }
    }
}