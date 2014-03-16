using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels
{
    public interface IHomePage
    {
        string SectionTitle { get; }
    }

    public class HomeViewModel
        : BaseViewModel
    {
        public List<IViewModel> Pages { get; private set; }

        public const double TileWidth = 400; //336;
        public const double TileHeight = TileWidth * 9 / 16;
        public const int TileMargin = 2;
        public const double SectionSpacing = 50;

        public static Thickness TileMarginThickness = new Thickness(TileMargin);

        public Func<object, object> TitleSelector
        {
            get { return item => ((IHomePage) item).SectionTitle; }
        }

        public HomeViewModel(ITheaterApplicationHost appHost)
        {
            var pageGenerators = appHost.GetExports<IHomePageGenerator>();
            Pages = pageGenerators.SelectMany(p => p.GetHomePages()).ToList();
        }
    }
}