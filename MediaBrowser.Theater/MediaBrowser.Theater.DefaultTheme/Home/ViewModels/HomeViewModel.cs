using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels
{
    public interface IHomePage : IViewModel
    {
        string SectionTitle { get; }
        int Index { get; set; }
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

        private readonly IEnumerable<IHomePageGenerator> _generators;

        public Func<object, object> TitleSelector
        {
            get { return item => ((IHomePage) item).SectionTitle; }
        }

        public HomeViewModel(ITheaterApplicationHost appHost)
        {
            _generators = appHost.GetExports<IHomePageGenerator>();
            
        }

        public override async Task Initialize()
        {
            var pageTasks = _generators.Select(p => p.GetHomePages()).ToList();

            await Task.WhenAll(pageTasks);

            var pages = pageTasks.SelectMany(t => t.Result).ToList();
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].Index = i;
            }

            Pages = pages.Cast<IViewModel>().ToList();

            await base.Initialize();
        }
    }
}