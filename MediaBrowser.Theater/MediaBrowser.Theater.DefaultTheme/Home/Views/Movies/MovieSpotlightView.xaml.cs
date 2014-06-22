using System.Windows.Input;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.Views.Movies
{
    /// <summary>
    ///     Interaction logic for MovieSpotlightView.xaml
    /// </summary>
    public partial class MovieSpotlightView
    {
        public MovieSpotlightView()
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