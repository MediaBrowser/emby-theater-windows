using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
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
        private readonly ILogger _logger;

        public List<IViewModel> Pages { get; private set; }

        public const double HeaderHeight = 100;
        public const double FooterHeight = 100;
        public const double TileWidth = TileHeight*(16/9); //400; //336;
        public const double TileHeight = (int)((1080 - 20 - 38 - 20 - HeaderHeight - FooterHeight - TileMargin*2*3) / 3); // 1080 - drag bar - command bar // TileWidth * 9 / 16;
        public const int TileMargin = 2;
        public const double SectionSpacing = 50;

        public static Thickness TileMarginThickness = new Thickness(TileMargin);

        private readonly IEnumerable<IHomePageGenerator> _generators;

        public Func<object, object> TitleSelector
        {
            get { return item => ((IHomePage) item).SectionTitle; }
        }

        public HomeViewModel(ITheaterApplicationHost appHost, ILogManager logManager)
        {
            _logger = logManager.GetLogger("HomeViewModel");
            _generators = appHost.GetExports<IHomePageGenerator>();
        }

        public override async Task Initialize()
        {
            var pageTasks = _generators.Select(p => p.GetHomePages()).ToList();

            try {
                await Task.WhenAll(pageTasks);
            }
            catch { }

            var pages = pageTasks.SelectMany(t => {
                try {
                    return t.Result;
                }
                catch (Exception ex) {
                    _logger.ErrorException("Home page generator", ex);
                    return Enumerable.Empty<IHomePage>();
                }
            }).ToList();

            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].Index = i;
            }

            Pages = pages.Cast<IViewModel>().ToList();

            await base.Initialize();
        }
    }
}