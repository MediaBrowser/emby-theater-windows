using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class LoginViewModel
        : BaseViewModel
    {
        private readonly ObservableCollection<IViewModel> _users;

        public ListCollectionView Users { get; private set; }

        public LoginViewModel(ISessionManager session, ILogManager logManager)
        {
            _users = new ObservableCollection<IViewModel> { new ManualLoginViewModel(session, logManager) };
            Users = new ListCollectionView(_users);
        }
    }
}
