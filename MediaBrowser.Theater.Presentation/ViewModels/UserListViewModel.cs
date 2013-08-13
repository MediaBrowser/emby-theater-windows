using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class UserListViewModel : BaseViewModel
    {
        public IPresentationManager PresentationManager { get; private set; }
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }

        private readonly RangeObservableCollection<UserDtoViewModel> _listItems =
            new RangeObservableCollection<UserDtoViewModel>();
        
        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get { return _listCollectionView; }

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

        private readonly ListBox _listBox;

        public UserListViewModel(IPresentationManager presentationManager, IApiClient apiClient, IImageManager imageManager, ListBox listBox)
        {
            ImageManager = imageManager;
            _listBox = listBox;
            ApiClient = apiClient;
            PresentationManager = presentationManager;
            ListCollectionView = new ListCollectionView(_listItems);

            ReloadUsers(true);
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

                _listItems.AddRange(users.Select(i => new UserDtoViewModel(ApiClient, ImageManager) { User = i }));

                if (selectedIndex.HasValue)
                {
                    //new ListFocuser(_listBox).FocusAfterContainersGenerated(selectedIndex.Value);
                    ListCollectionView.MoveCurrentToPosition(selectedIndex.Value);
                }
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }
    }
}
