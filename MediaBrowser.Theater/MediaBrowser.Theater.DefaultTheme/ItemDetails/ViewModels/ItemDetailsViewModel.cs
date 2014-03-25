using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemDetailsViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;
        private readonly IEnumerable<IItemDetailSection> _sections;

        public ItemDetailsViewModel(BaseItemDto item, IEnumerable<IItemDetailSection> sections)
        {
            _item = item;
            _sections = sections.ToList();
        }

        public IEnumerable<IItemDetailSection> Sections
        {
            get { return _sections; }
        }
    }

    public class ItemInfoSectionViewModel
        : BaseViewModel, IItemDetailSection
    {
        public ItemArtworkViewModel Image { get; private set; }
        public ItemInfoDetailsViewModel Details { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public bool IsPlayable { get; private set; }
        public ICommand PlayAllCommand { get; private set; }
        public bool HasPlayAll { get; private set; }

        public int SortOrder
        {
            get { return 0; }
        }

        public ItemInfoSectionViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager)
        {
            Image = new ItemArtworkViewModel(item, apiClient, imageManager);
            Details = new ItemInfoDetailsViewModel(item);
        }

        public override async Task Initialize()
        {
            await Image.Initialize();
            await Details.Initialize();
            await base.Initialize();
        }
    }
}