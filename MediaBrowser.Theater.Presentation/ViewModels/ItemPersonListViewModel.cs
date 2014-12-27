using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class ItemPersonListViewModel : BaseViewModel, IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IPresentationManager _presentationManager;
        private readonly INavigationService _navigation;

        public ICommand NavigateCommand { get; private set; }

        private readonly RangeObservableCollection<ItemPersonViewModel> _listItems =
         new RangeObservableCollection<ItemPersonViewModel>();

        private BaseItemDto _item;
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                var changed = _item != value;
                _item = value;

                if (changed)
                {
                    OnPropertyChanged("Item");

                    ReloadList(_item);
                }
            }
        }

        public ViewType ViewType { get; set; }

        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get
            {
                return _listCollectionView;
            }

            private set
            {
                var changed = _listCollectionView != value;
                _listCollectionView = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        public ItemPersonListViewModel(IApiClient apiClient, IImageManager imageManager, IPresentationManager presentationManager, INavigationService navigation)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _presentationManager = presentationManager;
            _navigation = navigation;
            _listCollectionView = new ListCollectionView(_listItems);

            NavigateCommand = new RelayCommand(Navigate);
        }

        private int? _imageWidth;
        public int? ImageWidth
        {
            get { return _imageWidth; }

            set
            {
                var changed = _imageWidth != value;
                _imageWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageWidth");
                }
            }
        }

        private int? _imageHeight;
        public int? ImageHeight
        {
            get { return _imageHeight; }

            set
            {
                var changed = _imageHeight != value;
                _imageHeight = value;

                if (changed)
                {
                    OnPropertyChanged("ImageHeight");
                }
            }
        }

        private void ReloadList(BaseItemDto item)
        {
            if (item == null)
            {
                _listItems.Clear();
                return;
            }

            // Record the current item
            var currentItem = _listCollectionView.CurrentItem as ItemPersonViewModel;

            var people = item.People ?? new BaseItemPerson[] { };

            people = people.Where(i => i.HasPrimaryImage).ToArray();

            int? selectedIndex = null;

            if (currentItem != null)
            {
                var index = Array.FindIndex(people, i => string.Equals(i.Name, currentItem.Name, StringComparison.OrdinalIgnoreCase));

                if (index != -1)
                {
                    selectedIndex = index;
                }
            }

            _listItems.Clear();

            _listItems.AddRange(people.Select(i => new ItemPersonViewModel(i, _apiClient, _imageManager)
            {
                ImageWidth = ImageWidth,
                ImageHeight = ImageHeight
            }));

            if (selectedIndex.HasValue)
            {
                ListCollectionView.MoveCurrentToPosition(selectedIndex.Value);
            }
        }

        private async void Navigate(object commandParameter)
        {
            var person = (ItemPersonViewModel)commandParameter;

            try
            {
                await _navigation.NavigateToPerson(person.Id, ViewType, Item.Id);
            }
            catch
            {
                _presentationManager.ShowDefaultErrorMessage();
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
