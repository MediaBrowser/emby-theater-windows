using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class BaseHomePageSectionViewModel : BaseViewModel
    {
        protected readonly IPresentationManager PresentationManager;
        protected readonly IApiClient ApiClient;

        protected const int TileMargin = 5;

        public double TileHeight { get; set; }
        public double TileWidth { get; set; }

        private BaseItemDto[] _backdropItems;

        private BaseItemDto _rootFolder;

        public BaseHomePageSectionViewModel(IPresentationManager presentationManager, IApiClient apiClient)
        {
            PresentationManager = presentationManager;
            ApiClient = apiClient;
        }

        protected async Task<BaseItemDto> GetRootFolder()
        {
            if (_rootFolder == null)
            {
                _rootFolder = await ApiClient.GetRootFolderAsync(ApiClient.CurrentUserId);
            }

            return _rootFolder;
        }

        protected async void NavigateWithLoading(Func<Task> navTask)
        {
            await navTask();
        }

        public BaseItemDto[] BackdropItems
        {
            get { return _backdropItems; }
            set
            {
                _backdropItems = value;
                OnPropertyChanged("BackdropItems");
            }
        }

        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (string.Equals(name, "BackdropItems"))
            {
                SetBackdrops();
            }
        }

        public void SetBackdrops()
        {
            var items = BackdropItems ?? new BaseItemDto[] { };

            PresentationManager.SetBackdrops(items.Select(i => ApiClient.GetImageUrl(i, new ImageOptions
            {
                ImageType = ImageType.Backdrop
            })));
        }

        public List<TabItem> AlphabetIndex
        {
            get
            {
                var list = new List<TabItem>();

                var chars = new[] { "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                list.AddRange(chars.Select(i => new TabItem { Name = i, DisplayName = i, TabType = "Alphabet" }));

                return list;
            }
        }
    }
}
