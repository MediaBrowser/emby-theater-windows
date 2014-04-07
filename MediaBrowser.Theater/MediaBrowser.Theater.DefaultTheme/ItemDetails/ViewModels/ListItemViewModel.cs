using System.Globalization;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ListItemViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;

        public ListItemViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager, INavigator navigator)
        {
            _item = item;

            ImageType[] preferredImageTypes;
            if (item.Type == "Episode") {
                preferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb };
            } else {
                preferredImageTypes = new[] { ImageType.Thumb, ImageType.Backdrop, ImageType.Art, ImageType.Screenshot, ImageType.Primary };
            }

            Artwork = new ItemArtworkViewModel(item, apiClient, imageManager) {
                DesiredImageHeight = 100,
                DesiredImageWidth = 178,
                PreferredImageTypes = preferredImageTypes
            };

            NavigateCommand = new RelayCommand(arg => navigator.Navigate(Go.To.Item(item)));
        }

        public ItemArtworkViewModel Artwork { get; private set; }

        public ICommand NavigateCommand { get; private set; }

        public string Name
        {
            get { return _item.Name; }
        }

        public string Detail
        {
            get
            {
                if (_item.Type == "Episode") {
                    if (_item.IndexNumber == null) {
                        return null;
                    }

                    if (_item.IndexNumberEnd != null) {
                        return string.Format("S{0}, E{1}-{2}", _item.ParentIndexNumber, _item.IndexNumber, _item.IndexNumberEnd);
                    }

                    return string.Format("S{0}, E{1}", _item.ParentIndexNumber, _item.IndexNumber);
                }

                if (_item.ProductionYear != null) {
                    return _item.ProductionYear.Value.ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }

        public float Rating
        {
            get { return _item.CommunityRating ?? 0; }
        }

        public bool HasRating
        {
            get { return _item.CommunityRating != null; }
        }
    }
}