using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.SortModeMenu.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SortModeMenu.Commands
{
    public class SortModeCommand
        : IGlobalMenuCommand
    {
        private readonly IPresenter _presenter;

        public SortModeCommand(IPresenter presenter, INavigator navigator)
        {
            _presenter = presenter;
            ExecuteCommand = new RelayCommand(arg => navigator.Navigate(Go.To.SortMode()));
        }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:SortModeCommand".Localize(); }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new SortModeCommandViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.ViewSettings; }
        }

        public int SortOrder
        {
            get { return 0; }
        }

        public bool EvaluateVisibility(INavigationPath currentPath)
        {
            return _presenter.CurrentPage is IHasItemSortModes;
        }
    }
    
    public class SortModeCommandViewModel : BaseViewModel { }
}