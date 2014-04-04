using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
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

        public bool ShowInfo
        {
            get { return !_item.IsFolder || !string.IsNullOrEmpty(_item.Overview); }
        }

        public ItemOverviewViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager)
        {
            _item = item;

            Artwork = new ItemArtworkViewModel(item, apiClient, imageManager) { DesiredImageHeight = 700 };
            Info = new ItemInfoViewModel(item);

            if (item.Type == "Episode")
                Artwork.PreferredImageTypes = new[] { ImageType.Screenshot, ImageType.Art, ImageType.Primary };

            Artwork.PropertyChanged += (s, e) => {
                if (e.PropertyName == "Size") {
                    OnPropertyChanged("Size");
                }
            };
        }

        public Size Size
        {
            get
            {
                var artWidth = Math.Min(1200, Artwork.ActualWidth);

                if (ShowInfo)
                    return new Size(artWidth + 600 + 20, 700);
                else
                    return new Size(artWidth, 700);
            }
        }
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

        public Task<IItemDetailSection> GetSection(BaseItemDto item)
        {
            IItemDetailSection section = new ItemOverviewViewModel(item, _apiClient, _imageManager);
            return Task.FromResult(section);
        }
    }
}