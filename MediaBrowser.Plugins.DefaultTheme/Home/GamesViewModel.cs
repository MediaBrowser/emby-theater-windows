using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Presentation;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class GamesViewModel : BaseHomePageSectionViewModel
    {
        public GamesViewModel(IPresentationManager presentationManager, IApiClient apiClient)
            : base(presentationManager, apiClient)
        {
        }
    }
}
