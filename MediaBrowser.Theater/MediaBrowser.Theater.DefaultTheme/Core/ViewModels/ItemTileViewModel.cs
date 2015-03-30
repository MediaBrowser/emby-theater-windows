using System;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemTileViewModel
        : BaseViewModel, IKnownSize
    {
        private BaseItemDto _item;
        private readonly IConnectionManager _connectionManager;
        private readonly IPlaybackManager _playbackManager;

        private bool? _showHeader;
        private ICommand _playCommand;
        private ICommand _goToDetailsCommand;
        private ICommand _playTrailerCommand;

        public ItemTileViewModel(IConnectionManager connectionManager, IImageManager imageManager,
                                 INavigator navigator, IPlaybackManager playbackManager, BaseItemDto item)
        {
            _connectionManager = connectionManager;
            _playbackManager = playbackManager;
            _item = item;

            Image = new ItemArtworkViewModel(item, connectionManager, imageManager);
            Image.PreferredImageTypes = new[] { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };
            Image.EnforcePreferredImageAspectRatio = true;
            Image.PropertyChanged += (senger, args) => {
                if (args.PropertyName == "Size") {
                    OnPropertyChanged("Size");
                }
            };

            DisplayNameGenerator = i => i.GetDisplayName(new DisplayNameFormat(true, true));
            GoToDetailsCommand = new RelayCommand(o => navigator.Navigate(Go.To.Item(Item)));
            PlayCommand = new RelayCommand(o => _playbackManager.Play(Media.Resume(item)));
        }

        public BaseItemDto Item
        {
            get { return _item; }
            set
            {
                if (Equals(_item, value)) {
                    return;
                }

                if (_item != null) {
                    var apiClient = _connectionManager.GetApiClient(_item);
                    apiClient.UserDataChanged -= serverEvents_UserDataChanged;
                }

                _item = value;

                if (_item != null) {
                    var apiClient = _connectionManager.GetApiClient(_item);
                    apiClient.UserDataChanged += serverEvents_UserDataChanged;
                }

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
                    return _item.UserData.PlayedPercentage ?? 0;
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

        public ICommand PlayCommand
        {
            get { return _playCommand; }
            set
            {
                if (Equals(value, _playCommand)) {
                    return;
                }
                _playCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand GoToDetailsCommand
        {
            get { return _goToDetailsCommand; }
            set
            {
                if (Equals(value, _goToDetailsCommand)) {
                    return;
                }
                _goToDetailsCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand PlayTrailerCommand
        {
            get { return _playTrailerCommand; }
            set
            {
                if (Equals(value, _playTrailerCommand)) {
                    return;
                }
                _playTrailerCommand = value;
                OnPropertyChanged();
            }
        }

        public Size Size
        {
            get { return new Size(Image.ActualWidth + 2*HomeViewModel.TileMargin, Image.ActualHeight + 2*HomeViewModel.TileMargin); }
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
}