using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInput;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api
{
    public interface ITheaterApplicationHost
        : IApplicationHost
    {
        ITheaterConfigurationManager TheaterConfigurationManager { get; }

        INavigator Navigator { get; }
        IPresenter Presenter { get; }
        IUserInputManager UserInputManager { get; }
    }
}