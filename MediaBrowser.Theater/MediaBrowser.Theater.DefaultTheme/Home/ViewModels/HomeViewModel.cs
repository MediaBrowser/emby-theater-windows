using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels
{
    public class HomeViewModel
        : BaseViewModel
    {
        public List<IPanoramaPage> Pages { get; private set; }

        public const double TileWidth = 336;
        public const double TileHeight = TileWidth * 9 / 16;
        public const int TileMargin = 5;

        public List<IPanoramaPage> TitlePages
        {
            get { return Pages.Where(p => p.IsTitlePage).ToList(); }
        }

        public HomeViewModel(ITheaterApplicationHost appHost)
        {
            var pageGenerators = appHost.GetExports<IHomePageGenerator>();
            Pages = pageGenerators.SelectMany(p => p.GetHomePages()).ToList();
        }
    }
}