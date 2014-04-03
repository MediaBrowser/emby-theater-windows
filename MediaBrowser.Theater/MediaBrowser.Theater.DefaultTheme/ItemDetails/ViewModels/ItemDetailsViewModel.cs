using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemDetailsViewModel
        : BaseViewModel, IRootPresentationOptions
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

        public bool ShowMediaBrowserLogo
        {
            get { return false; }
        }

        public bool ShowCommandBar
        {
            get { return true; }
        }

        public bool ShowClock
        {
            get { return true; }
        }

        public string Title
        {
            get { return ItemTileViewModel.GetDisplayNameWithAiredSpecial(_item); }
        }
    }
}