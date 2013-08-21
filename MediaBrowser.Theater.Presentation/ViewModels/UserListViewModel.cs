using MediaBrowser.Model.ApiClient;
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

        private readonly RangeObservableCollection<UserDtoViewModel> _listItems =
            new RangeObservableCollection<UserDtoViewModel>();
        
        private ListCollectionView _users;
        public ListCollectionView Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new ListCollectionView(_listItems);
                    ReloadUsers(true);
                }

                return _users;
            }

            set
            {
                var changed = _users != value;
                _users = value;

                if (changed)
                {
                    OnPropertyChanged("Users");
                }
            }
        }

        public UserListViewModel(IPresentationManager presentationManager, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager)
        {
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            PresentationManager = presentationManager;
        }

        private async void ReloadUsers(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = _users.CurrentItem as UserDtoViewModel;

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

                _listItems.AddRange(users.Select(i => new UserDtoViewModel(ApiClient, ImageManager, SessionManager) { User = i }));

                if (selectedIndex.HasValue)
                {
                    Users.MoveCurrentToPosition(selectedIndex.Value);
                }
            }
            catch (Exception)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
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
