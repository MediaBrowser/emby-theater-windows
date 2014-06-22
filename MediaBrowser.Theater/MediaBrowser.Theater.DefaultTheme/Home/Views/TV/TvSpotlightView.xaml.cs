using System.Windows.Input;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.Views.TV
{
    /// <summary>
    ///     Interaction logic for TvSpotlightView.xaml
    /// </summary>
    public partial class TvSpotlightView
    {
        public TvSpotlightView()
        {
            InitializeComponent();

            Loaded += (s, e) => {
                var page = DataContext as IHomePage;
                if (page != null && page.Index == 0) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }
            };
        }
    }
}