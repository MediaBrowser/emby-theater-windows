using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for Trailers.xaml
    /// </summary>
    public partial class Trailers : BaseItemsControl
    {
        private readonly BaseItemDto _item;

        private readonly IPlaybackManager _playbackManager;

        public Trailers(Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow, BaseItemDto item, IPlaybackManager playbackManager)
            : base(displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            _item = item;
            _playbackManager = playbackManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var item = (BaseItemDtoViewModel)e.Argument;

            await _playbackManager.Play(new PlayOptions(item.Item));
        }

        protected override ExtendedListBox ItemsList
        {
            get { return LstItems; }
        }

        protected override async Task<ItemsResult> GetItemsAsync()
        {
            try
            {
                var items = await ApiClient.GetLocalTrailersAsync(SessionManager.CurrentUser.Id, _item.Id);

                return new ItemsResult
                {
                    Items = items,
                    TotalRecordCount = items.Length
                };
            }
            catch (HttpException)
            {
                return new ItemsResult();
            }
        }

        protected override bool SetBackdropsOnCurrentItemChanged
        {
            get
            {
                return false;
            }
        }

        protected override BaseItemDtoViewModel CreateViewModel(BaseItemDto item, double medianPrimaryImageAspectRatio)
        {
            var vm = base.CreateViewModel(item, medianPrimaryImageAspectRatio);

            vm.IsLocalTrailer = true;

            return vm;
        }

        protected override double GetImageDisplayHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (medianPrimaryImageAspectRatio.Equals(1))
            {
                medianPrimaryImageAspectRatio = 1.777777777777778;
            }

            double height = displayPreferences.PrimaryImageWidth;

            return height / medianPrimaryImageAspectRatio;
        }
    }
}
