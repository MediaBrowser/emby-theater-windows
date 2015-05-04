using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Channels
{
    public class ChannelListViewModel
        : BaseViewModel, IKnownSize, IHomePage
    {
        private readonly ItemTileFactory _itemFactory;
        private readonly ISessionManager _sessionManager;

        private bool _isVisible;

        public ChannelListViewModel(ISessionManager sessionManager, ItemTileFactory itemFactory)
        {
            _sessionManager = sessionManager;
            _itemFactory = itemFactory;

            Title = SectionTitle = "MediaBrowser.Theater.DefaultTheme:Strings:Home_Channels_Title".Localize();

            Channels = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 8; i++) {
                Channels.Add(CreateChannelItem());
            }

            IsVisible = true;
            LoadItems();
        }

        public RangeObservableCollection<ItemTileViewModel> Channels { get; private set; }

        public string Title { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (Equals(_isVisible, value)) {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public string SectionTitle { get; set; }

        public int Index { get; set; }

        public Size Size
        {
            get
            {
                if (Channels.Count == 0) {
                    return new Size();
                }

                var width = (int) Math.Ceiling(Channels.Count/3.0);

                return new Size(width*(HomeViewModel.TileWidth + 2*HomeViewModel.TileMargin) + HomeViewModel.SectionSpacing,
                                3*(HomeViewModel.TileHeight + 2*HomeViewModel.TileMargin));
            }
        }

        private async void LoadItems()
        {
            IApiClient apiClient = _sessionManager.ActiveApiClient;

            QueryResult<BaseItemDto> channels = await apiClient.GetChannels(new ChannelQuery {
                UserId = _sessionManager.CurrentUser.Id
            });

            BaseItemDto[] items = channels.Items;

            for (int i = 0; i < items.Length; i++) {
                if (Channels.Count > i) {
                    Channels[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateChannelItem();
                    vm.Item = items[i];

                    Channels.Add(vm);
                }
            }

            if (Channels.Count > items.Length) {
                List<ItemTileViewModel> toRemove = Channels.Skip(items.Length).ToList();
                Channels.RemoveRange(toRemove);
            }

            IsVisible = Channels.Count > 0;
            OnPropertyChanged("Size");
        }

        private ItemTileViewModel CreateChannelItem()
        {
            ItemTileViewModel vm = _itemFactory.Create(null);
            vm.DesiredImageWidth = HomeViewModel.TileWidth;
            vm.DesiredImageHeight = HomeViewModel.TileHeight;
            vm.ShowCaptionBar = false;
            vm.PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary };
            return vm;
        }
    }
}