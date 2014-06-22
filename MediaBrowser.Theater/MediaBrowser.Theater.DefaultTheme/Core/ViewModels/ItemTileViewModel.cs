using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemTileViewModel
        : BaseViewModel, IKnownSize
    {
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private BaseItemDto _item;
        private readonly IPlaybackManager _playbackManager;

        private bool? _showHeader;

        public ItemTileViewModel(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents,
                                 INavigator navigator, IPlaybackManager playbackManager, BaseItemDto item)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _playbackManager = playbackManager;
            _item = item;

            Image = new ItemArtworkViewModel(item, apiClient, imageManager);
            Image.PreferredImageTypes = new[] { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };
            Image.EnforcePreferredImageAspectRatio = true;
            Image.PropertyChanged += (senger, args) => {
                if (args.PropertyName == "Size") {
                    OnPropertyChanged("Size");
                    OnPropertyChanged("ShowDisplayName");
                }
            };

            DisplayNameGenerator = GetDisplayNameWithAiredSpecial;
            GoToDetailsCommand = new RelayCommand(async o => navigator.Navigate(Go.To.Item(Item)));
            PlayCommand = new RelayCommand(o => _playbackManager.Play(new PlayOptions(Item) { GoFullScreen = true, EnableCustomPlayers = true, Resume = true }));

            serverEvents.UserDataChanged += serverEvents_UserDataChanged;
        }

        public BaseItemDto Item
        {
            get { return _item; }
            set
            {
                if (Equals(_item, value)) {
                    return;
                }

                _item = value;
                OnPropertyChanged();
                RefreshItemFields();
                Image.Item = value;
            }
        }

        public double? DesiredImageWidth
        {
            get { return Image.DesiredImageWidth; }
            set { Image.DesiredImageWidth = value; }
        }

        public double? DesiredImageHeight
        {
            get { return Image.DesiredImageHeight; }
            set { Image.DesiredImageHeight = value; }
        }

        public ImageType[] PreferredImageTypes
        {
            get { return Image.PreferredImageTypes; }
            set { Image.PreferredImageTypes = value; }
        }

        public string DisplayName
        {
            get { return _item == null ? string.Empty : DisplayNameGenerator(_item); }
        }

        public bool ShowCaptionBar
        {
            get { return _showHeader ?? ShouldShowDisplayNameByImageType(); }
            set { _showHeader = value; }
        }
        
        public bool IsFolder
        {
            get { return _item != null && _item.IsFolder; }
        }

        public Func<BaseItemDto, string> DisplayNameGenerator { get; set; }

        public string Creator
        {
            get { return _item == null ? string.Empty : _item.AlbumArtist; }
        }

        public bool HasCreator
        {
            get { return _item != null && !string.IsNullOrEmpty(_item.AlbumArtist); }
        }

        public bool IsPlayed
        {
            get { return _item != null && _item.UserData.Played; }
        }

        public bool IsInProgress
        {
            get
            {
                double percent = PlayedPercent;
                return percent > 0 && percent < 100;
            }
        }

        public double PlayedPercent
        {
            get
            {
                if (_item == null) {
                    return 0;
                }

                if (_item.IsFolder) {
                    return _item.PlayedPercentage ?? 0;
                }

                if (_item.RunTimeTicks.HasValue) {
                    if (_item.UserData != null && _item.UserData.PlaybackPositionTicks > 0) {
                        double percent = _item.UserData.PlaybackPositionTicks;
                        percent /= _item.RunTimeTicks.Value;

                        return percent*100;
                    }
                }

                return 0;
            }
        }

        public float CommunityRating
        {
            get { return _item.CommunityRating ?? 0; }
        }

        public bool HasCommunityRating
        {
            get { return _item.CommunityRating != null; }
        }

        public ItemArtworkViewModel Image { get; private set; }

        public ICommand PlayCommand { get; private set; }
        public ICommand GoToDetailsCommand { get; private set; }
        public ICommand PlayTrailerCommand { get; private set; }

        public Size Size
        {
            get { return new Size(Image.ActualWidth + 2*HomeViewModel.TileMargin, Image.ActualHeight + 2*HomeViewModel.TileMargin); }
        }

        public static string GetDisplayName(BaseItemDto item)
        {
            string name = item.Name;

            if (item.IndexNumber.HasValue && !item.IsType("season")) {
                name = item.IndexNumber + " - " + name;
            }

            if (item.ParentIndexNumber.HasValue && item.IsAudio) {
                name = item.ParentIndexNumber + "." + name;
            }

            return name;
        }

        public static string GetDisplayNameWithAiredSpecial(BaseItemDto item)
        {
            if (item == null) {
                return string.Empty;
            }

            if (item.IsType("episode") && item.ParentIndexNumber.HasValue && item.ParentIndexNumber.Value == 0) {
                return "Special - " + item.Name;
            }

            return GetDisplayName(item);
        }

        private bool ShouldShowDisplayNameByImageType()
        {
            double aspectRatio = Image.ActualWidth/Image.ActualHeight;
            return aspectRatio >= 1;
        }

        private void serverEvents_UserDataChanged(object sender, GenericEventArgs<UserDataChangeInfo> e)
        {
            RefreshUserDataFields();
        }

        private void RefreshUserDataFields()
        {
            OnPropertyChanged("PlayedPercent");
            OnPropertyChanged("IsInProgress");
            OnPropertyChanged("IsPlayed");
            OnPropertyChanged("ShowCaptionBar");
        }

        private void RefreshItemFields()
        {
            OnPropertyChanged("DisplayName");
            OnPropertyChanged("IsFolder");
            OnPropertyChanged("Creator");
            OnPropertyChanged("HasCreator");
            OnPropertyChanged("IsPlayed");
            OnPropertyChanged("IsInProgress");
            OnPropertyChanged("PlayedPercent");
            OnPropertyChanged("ShowCaptionBar");
        }
    }

    public class ItemListElementViewModel
        : ItemTileViewModel 
    {
        public ItemListElementViewModel(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, IPlaybackManager playbackManager, BaseItemDto item) 
            : base(apiClient, imageManager, serverEvents, navigator, playbackManager, item) { }

        public string Detail
        {
            get
            {
                if (Item.Type == "Episode")
                {
                    if (Item.IndexNumber == null)
                    {
                        return null;
                    }

                    if (Item.IndexNumberEnd != null)
                    {
                        return string.Format("S{0}, E{1}-{2}", Item.ParentIndexNumber, Item.IndexNumber, Item.IndexNumberEnd);
                    }

                    return string.Format("S{0}, E{1}", Item.ParentIndexNumber, Item.IndexNumber);
                }

                if (Item.ProductionYear != null)
                {
                    return Item.ProductionYear.Value.ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }
    }
}