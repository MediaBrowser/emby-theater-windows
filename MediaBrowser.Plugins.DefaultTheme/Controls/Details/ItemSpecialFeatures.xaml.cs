using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Linq;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Interaction logic for ItemSpecialFeatures.xaml
    /// </summary>
    public partial class ItemSpecialFeatures : BaseDetailsControl
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;
        private readonly IThemeManager _themeManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSpecialFeatures" /> class.
        /// </summary>
        public ItemSpecialFeatures(IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IThemeManager themeManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _sessionManager = sessionManager;
            _themeManager = themeManager;
            InitializeComponent();

            lstItems.ItemInvoked += lstItems_ItemInvoked;
        }

        /// <summary>
        /// LSTs the items_ item invoked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void lstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var viewModel = (SpecialFeatureViewModel) e.Argument;

            //UIKernel.Instance.PlaybackManager.Play(new PlayOptions
            //{
            //    Items = new List<BaseItemDto> { viewModel.Item }
            //});
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected override async void OnItemChanged()
        {
            BaseItemDto[] result;

            try
            {
                result = await _apiClient.GetSpecialFeaturesAsync(_sessionManager.CurrentUser.Id, Item.Id);
            }
            catch (HttpException)
            {
                _themeManager.CurrentTheme.ShowDefaultErrorMessage();

                return;
            }

            var resultItems = result ?? new BaseItemDto[] { };

            const int height = 297;
            var aspectRatio = BaseItemDtoViewModel.GetAveragePrimaryImageAspectRatio(resultItems);

            var width = aspectRatio.Equals(0) ? 528 : height * aspectRatio;

            lstItems.ItemsSource = resultItems.Select(i => new SpecialFeatureViewModel(_apiClient, _imageManager)
            {
                Item = i,
                ImageHeight = height,
                ImageWidth = width

            }).ToList();
        }
    }
}
