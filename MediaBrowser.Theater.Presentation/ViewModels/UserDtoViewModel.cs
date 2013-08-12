using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class UserDtoViewModel
    /// </summary>
    public class UserDtoViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public IApiClient ApiClient { get; private set; }
        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        public IImageManager ImageManager { get; private set; }

        /// <summary>
        /// The _item
        /// </summary>
        private UserDto _item;
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user.</value>
        public UserDto User
        {
            get { return _item; }

            set
            {
                var changed = _item != value;

                _item = value;

                if (changed)
                {
                    OnPropertyChanged("User");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDtoViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageManager">The image manager.</param>
        public UserDtoViewModel(IApiClient apiClient, IImageManager imageManager)
        {
            ApiClient = apiClient;
            ImageManager = imageManager;
        }
    }
}
