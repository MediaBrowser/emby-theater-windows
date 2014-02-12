using System.Threading.Tasks;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class HelloWorldViewModel
        : BaseViewModel
    {
        public override async Task Initialize()
        {
            await base.Initialize();
        }
    }
}