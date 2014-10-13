using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels
{
    public class SideMenuViewModel
        : BaseViewModel
    {
        private readonly ISessionManager _sessionManager;

        public SideMenuViewModel(ITheaterApplicationHost appHost, ISessionManager sessionManager, IImageManager imageManager)
        {
            _sessionManager = sessionManager;
            Users = new SideMenuUsersViewModel(sessionManager, imageManager);
            CommandGroups = new RangeObservableCollection<SideMenuGroupViewModel>();
            UserCommandGroups = new RangeObservableCollection<SideMenuGroupViewModel>();

            IEnumerable<IMenuCommand> commands = appHost.GetExports<IMenuCommand>().ToList();
            
            IEnumerable<IGrouping<MenuCommandGroup, IMenuCommand>> commandGroups = commands.Where(c => c.Group != MenuCommandGroup.User).Where(c => !(c is IGlobalMenuCommand)).GroupBy(c => c.Group);
            CommandGroups.AddRange(commandGroups.OrderBy(g => g.Key.SortOrder).Select(g => new SideMenuGroupViewModel(g)));

            IEnumerable<IGrouping<MenuCommandGroup, IMenuCommand>> userCommandGroups = commands.Where(c => c.Group == MenuCommandGroup.User).Where(c => !(c is IGlobalMenuCommand)).GroupBy(c => c.Group);
            UserCommandGroups.AddRange(userCommandGroups.OrderBy(g => g.Key.SortOrder).Select(g => new SideMenuGroupViewModel(g)));

            sessionManager.UserLoggedIn += UserChanged;
            sessionManager.UserLoggedOut += UserChanged;
        }

        void UserChanged(object sender, System.EventArgs e)
        {
            OnPropertyChanged("IsUserSignedIn");
        }

        public bool IsUserSignedIn
        {
            get { return _sessionManager.CurrentUser != null; }
        }

        public SideMenuUsersViewModel Users { get; private set; }
        public RangeObservableCollection<SideMenuGroupViewModel> UserCommandGroups { get; private set; }
        public RangeObservableCollection<SideMenuGroupViewModel> CommandGroups { get; private set; }
    }

    public class SideMenuGroupViewModel
        : BaseViewModel
    {
        public SideMenuGroupViewModel(IEnumerable<IMenuCommand> commands)
        {
            Buttons = new RangeObservableCollection<SideMenuButtonViewModel>();
            Buttons.AddRange(commands.OrderBy(c => c.SortOrder).Select(c => new SideMenuButtonViewModel(c)));
        }

        public RangeObservableCollection<SideMenuButtonViewModel> Buttons { get; private set; }
    }

    public class SideMenuButtonViewModel
        : BaseViewModel
    {
        private readonly IMenuCommand _command;

        public SideMenuButtonViewModel(IMenuCommand command)
        {
            _command = command;
        }

        public IViewModel Icon
        {
            get { return _command.IconViewModel; }
        }

        public string DisplayName
        {
            get { return _command.DisplayName; }
        }

        public ICommand ExecuteCommand
        {
            get { return _command.ExecuteCommand; }
        }
    }
}