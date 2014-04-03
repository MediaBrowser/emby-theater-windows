using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemOverviewViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly BaseItemDto _item;

        public ItemArtworkViewModel Artwork { get; set; }
        public ItemInfoViewModel Info { get; set; }

        public ICommand PlayCommand { get; set; }
        public ICommand EnqueueCommand { get; set; }
        public bool CanPlay { get; set; }

        public ICommand PlayAllCommand { get; set; }
        public ICommand EnqueueAllCommand { get; set; }
        public bool CanPlayAll { get; set; }

        public ICommand ResumeCommand { get; set; }

        public int SortOrder
        {
            get { return 0; }
        }

        public ItemOverviewViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager)
        {
            _item = item;

            Artwork = new ItemArtworkViewModel(item, apiClient, imageManager) { DesiredImageHeight = 800 };
            Info = new ItemInfoViewModel(item);

            Artwork.PropertyChanged += (s, e) => {
                if (e.PropertyName == "Size") {
                    OnPropertyChanged("Size");
                }
            };
        }

        public Size Size { get { return new Size(Artwork.ActualWidth + 600 + 20, 800); } }
    }

    public class ItemOverviewSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public ItemOverviewSectionGenerator(IApiClient apiClient, IImageManager imageManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item != null;
        }

        public IItemDetailSection GetSection(BaseItemDto item)
        {
            return new ItemOverviewViewModel(item, _apiClient, _imageManager);
        }
    }
}