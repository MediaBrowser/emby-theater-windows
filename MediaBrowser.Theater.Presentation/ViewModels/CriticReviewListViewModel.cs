using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class CriticReviewListViewModel : BaseViewModel
    {
        public IPresentationManager PresentationManager { get; private set; }
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }

        private readonly RangeObservableCollection<ItemReviewViewModel> _listItems =
            new RangeObservableCollection<ItemReviewViewModel>();

        private ListCollectionView _reviews;
        public ListCollectionView ListCollectionView
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
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        private readonly string _itemId;

        public CriticReviewListViewModel(IPresentationManager presentationManager, IApiClient apiClient, IImageManager imageManager, string itemId)
        {
            ImageManager = imageManager;
            _itemId = itemId;
            ApiClient = apiClient;
            PresentationManager = presentationManager;
        }

        private CancellationTokenSource _reviewsCancellationTokenSource;

        private async void ReloadReviews()
        {
            var cancellationTokenSource = _reviewsCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var result = await ApiClient.GetCriticReviews(_itemId, cancellationTokenSource.Token);

                _listItems.Clear();

                _listItems.AddRange(result.ItemReviews.Where(i => !string.IsNullOrEmpty(i.Caption)).Select(i => new ItemReviewViewModel { Review = i }));
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                cancellationTokenSource.Dispose();

                if (cancellationTokenSource == _reviewsCancellationTokenSource)
                {
                    _reviewsCancellationTokenSource = null;
                }
            }
        }

        public void Dispose()
        {
            if (_reviewsCancellationTokenSource != null)
            {
                _reviewsCancellationTokenSource.Dispose();
            }
        }
    }
}
