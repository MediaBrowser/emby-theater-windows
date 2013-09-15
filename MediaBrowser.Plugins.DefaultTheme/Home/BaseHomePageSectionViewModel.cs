using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class BaseHomePageSectionViewModel : BaseViewModel
    {
        protected readonly IPresentationManager PresentationManager;
        protected readonly IApiClient ApiClient;

        protected const int TilePadding = 36;

        public double TileHeight { get; set; }
        public double TileWidth { get; set; }

        private BaseItemDto[] _backdropItems;

        public BaseHomePageSectionViewModel(IPresentationManager presentationManager, IApiClient apiClient)
        {
            PresentationManager = presentationManager;
            ApiClient = apiClient;
        }

        protected async void NavigateWithLoading(Func<Task> navTask)
        {
            PresentationManager.ShowLoadingAnimation();

            try
            {
                await navTask();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
            }
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
    }
}
