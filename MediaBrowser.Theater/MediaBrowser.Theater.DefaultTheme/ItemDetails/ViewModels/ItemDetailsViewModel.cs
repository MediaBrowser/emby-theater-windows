using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemDetailsViewModel
        : BaseViewModel, IHasRootPresentationOptions
    {
        private readonly IEnumerable<IItemDetailSection> _sections;

        public ItemDetailsViewModel(BaseItemDto item, IEnumerable<IItemDetailSection> sections)
        {
            _sections = sections.ToList();

            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false,
                Title = ItemTileViewModel.GetDisplayNameWithAiredSpecial(item)
            };
        }

        public IEnumerable<IItemDetailSection> Sections
        {
            get { return _sections; }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }
    }
}