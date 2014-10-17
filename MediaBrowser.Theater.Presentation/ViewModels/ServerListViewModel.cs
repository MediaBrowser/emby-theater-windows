using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class ServerListViewModel : BaseViewModel
    {
        private readonly RangeObservableCollection<ServerInfoViewModel> _listItems =
            new RangeObservableCollection<ServerInfoViewModel>();

        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    _listCollectionView.CurrentChanged += ListCollectionViewCurrentChanged;
                    ReloadItems(true);
                }

                return _listCollectionView;
            }

            set
            {
                var changed = _listCollectionView != value;
                _listCollectionView = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        private ServerInfoViewModel _currentItem;
        public ServerInfoViewModel CurrentItem
        {
            get { return _currentItem; }

            set
            {
                var changed = _currentItem != value;

                _currentItem = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentItem");
                }
            }
        }

        private readonly List<ServerInfo> _servers;

        public ServerListViewModel(List<ServerInfo> servers)
        {
            _servers = servers;
        }

        private void ReloadItems(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = _listCollectionView.CurrentItem as ServerInfoViewModel;

            int? selectedIndex = null;

            if (isInitialLoad)
            {
                selectedIndex = 0;
            }
            else if (currentItem != null)
            {
                var index = Array.FindIndex(_servers.ToArray(), i => string.Equals(i.Id, currentItem.Server.Id));

                if (index != -1)
                {
                    selectedIndex = index;
                }
            }

            _listItems.Clear();

            _listItems.AddRange(_servers.Select(i => new ServerInfoViewModel() { Server = i }));

            if (selectedIndex.HasValue)
            {
                ListCollectionView.MoveCurrentToPosition(selectedIndex.Value);
            }
        }

        void ListCollectionViewCurrentChanged(object sender, EventArgs e)
        {
            CurrentItem = ListCollectionView.CurrentItem as ServerInfoViewModel;
        }

        public void Dispose()
        {
        }
    }
}
