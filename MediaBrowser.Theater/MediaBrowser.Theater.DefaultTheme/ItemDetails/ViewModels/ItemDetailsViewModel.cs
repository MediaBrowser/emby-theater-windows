using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemDetailsViewModel
        : BaseViewModel, IHasRootPresentationOptions, IItemDetailsViewModel
    {
        private readonly BaseItemDto _item;
        private readonly IEnumerable<IItemDetailSection> _sections;

        public BaseItemDto Item
        {
            get { return _item; }
        }

        public ItemDetailsViewModel(BaseItemDto item, IEnumerable<IItemDetailSection> sections)
        {
            _item = item;
            _sections = sections.ToList();

            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false,
                Title = item.GetDisplayName(new DisplayNameFormat(true, false))
            };
        }

        public IEnumerable<IItemDetailSection> Sections
        {
            get { return _sections; }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }
    }
}