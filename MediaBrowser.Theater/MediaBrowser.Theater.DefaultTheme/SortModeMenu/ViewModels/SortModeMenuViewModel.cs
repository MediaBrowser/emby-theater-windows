using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SortModeMenu.ViewModels
{
    public class SortModeMenuViewModel
        : BaseViewModel
    {
        private IHasItemSortModes _items;
        public IEnumerable<SortOptionViewModel> SortOptions { get; private set; }

        public IHasItemSortModes SortableItems
        {
            get { return _items; }
            set
            {
                if (Equals(value, _items)) {
                    return;
                }
                _items = value;
                OnPropertyChanged();
            }
        }

        public SortModeMenuViewModel(IHasItemSortModes items)
        {
            _items = items;
            
            SortOptions = items.AvailableSortModes.Select(sm => new SortOptionViewModel(items, sm));
            SortAscendingCommand = new RelayCommand(arg => _items.SortDirection = SortDirection.Ascending);
            SortDescendingCommand = new RelayCommand(arg => _items.SortDirection = SortDirection.Descending);
        }

        public ICommand SortAscendingCommand { get; private set; }
        public ICommand SortDescendingCommand { get; private set; }
    }

    public class SortModeMenuPath : NavigationPath { }

    public static class SortModeMenuPathExtensions
    {
        public static SortModeMenuPath SortMode(this Go go)
        {
            return new SortModeMenuPath();
        }
    }
}