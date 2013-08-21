using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class CriticReviewsViewModel : BaseViewModel
    {
        public IPresentationManager PresentationManager { get; private set; }
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }

        private readonly RangeObservableCollection<ItemReviewViewModel> _listItems =
            new RangeObservableCollection<ItemReviewViewModel>();

        private ListCollectionView _reviews;
        public ListCollectionView Reviews
        {
            get
            {
                if (_reviews == null)
                {
                    _reviews = new ListCollectionView(_listItems);
                    ReloadReviews();
                }

                return _reviews;
            }

            set
            {
                var changed = _reviews != value;
                _reviews = value;

                if (changed)
                {
                    OnPropertyChanged("Reviews");
                }
            }
        }

        private readonly string _itemId;

        public CriticReviewsViewModel(IPresentationManager presentationManager, IApiClient apiClient, IImageManager imageManager, string itemId)
        {
            ImageManager = imageManager;
            _itemId = itemId;
            ApiClient = apiClient;
            PresentationManager = presentationManager;
        }

        private async void ReloadReviews()
        {
            try
            {
                var result = await ApiClient.GetCriticReviews(_itemId);

                _listItems.Clear();

                _listItems.AddRange(result.ItemReviews.Select(i => new ItemReviewViewModel { Review = i }));

                if (result.ItemReviews.Length > 0)
                {
                    Reviews.MoveCurrentToPosition(0);
                }
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }

        public void Dispose()
        {
        }
    }
}
