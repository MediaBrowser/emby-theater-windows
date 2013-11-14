using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class UserListViewModel : BaseViewModel, IDisposable
    {
        public IPresentationManager PresentationManager { get; private set; }
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }
        public ISessionManager SessionManager { get; private set; }
        private readonly INavigationService _navigation;

        private readonly RangeObservableCollection<UserDtoViewModel> _listItems =
            new RangeObservableCollection<UserDtoViewModel>();
        
        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    _listCollectionView.CurrentChanged += ListCollectionViewCurrentChanged;
                    ReloadUsers(true);
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

        private UserDtoViewModel _currentItem;
        public UserDtoViewModel CurrentItem
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

        public UserListViewModel(IPresentationManager presentationManager, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigation)
        {
            SessionManager = sessionManager;
            _navigation = navigation;
            ImageManager = imageManager;
            ApiClient = apiClient;
            PresentationManager = presentationManager;
        }

        private async void ReloadUsers(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = _listCollectionView.CurrentItem as UserDtoViewModel;

            try
            {
                var users = await ApiClient.GetPublicUsersAsync();

                int? selectedIndex = null;

                if (isInitialLoad)
                {
                    selectedIndex = 0;
                }
                else if (currentItem != null)
                {
                    var index = Array.FindIndex(users, i => string.Equals(i.Id, currentItem.User.Id));

                    if (index != -1)
                    {
                        selectedIndex = index;
                    }
                }

                _listItems.Clear();

                _listItems.AddRange(users.Select(i => new UserDtoViewModel(ApiClient, ImageManager, SessionManager, _navigation) { User = i }));

                if (selectedIndex.HasValue)
                {
                    ListCollectionView.MoveCurrentToPosition(selectedIndex.Value);
                }
            }
            catch (Exception)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }

        void ListCollectionViewCurrentChanged(object sender, EventArgs e)
        {
            CurrentItem = ListCollectionView.CurrentItem as UserDtoViewModel;
        }

        public void Dispose()
        {
            foreach (var item in _listItems.ToList())
            {
                item.Dispose();
            }
        }
    }
}
