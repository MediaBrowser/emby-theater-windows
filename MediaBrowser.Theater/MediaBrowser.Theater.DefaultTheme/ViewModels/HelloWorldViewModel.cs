using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class HelloWorldViewModel
        : BaseViewModel
    {
        public override async Task Initialize()
        {
            await Task.Delay(5000);
            await base.Initialize();
        }
    }
}