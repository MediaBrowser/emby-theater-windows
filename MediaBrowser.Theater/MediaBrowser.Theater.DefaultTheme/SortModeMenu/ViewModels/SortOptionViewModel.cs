using System.ComponentModel;
using System.Windows.Input;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SortModeMenu.ViewModels
{
    public class SortOptionViewModel
        : BaseViewModel
    {
        private readonly IHasItemSortModes _items;
        private readonly IItemListSortMode _sortMode;

        public SortOptionViewModel(IHasItemSortModes items, IItemListSortMode sortMode)
        {
            _sortMode = sortMode;
            _items = items;

            SelectCommand = new RelayCommand(arg => _items.SortMode = _sortMode);

            var npc = items as INotifyPropertyChanged;
            if (npc != null) {
                npc.PropertyChanged += (s, e) => {
                    if (e.PropertyName == "SortMode") {
                        OnPropertyChanged("IsSelected");
                    }
                };
            }
        }

        public string DisplayName
        {
            get { return _sortMode.DisplayName; }
        }

        public bool IsSelected
        {
            get { return _items.SortMode == _sortMode; }
        }

        public ICommand SelectCommand { get; private set; }
    }
}