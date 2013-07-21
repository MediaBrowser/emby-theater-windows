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
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for ThemeVideos.xaml
    /// </summary>
    public partial class ThemeVideos : BaseItemsControl
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly BaseItemDto _parentItem;

        public ThemeVideos(Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow, IPlaybackManager playbackManager, BaseItemDto parentItem) 
            : base(displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            _playbackManager = playbackManager;
            _parentItem = parentItem;
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
            AllThemeMediaResult allThemeMedia;

            try
            {
                allThemeMedia = await ApiClient.GetAllThemeMediaAsync(SessionManager.CurrentUser.Id, _parentItem.Id, false).ConfigureAwait(false);
            }
            catch (HttpException)
            {
                // Don't let this blow up the page
                allThemeMedia = new AllThemeMediaResult();
            }

            return new ItemsResult
            {
                TotalRecordCount = allThemeMedia.ThemeSongsResult.TotalRecordCount + allThemeMedia.ThemeVideosResult.TotalRecordCount,
                Items = allThemeMedia.ThemeVideosResult.Items.Concat(allThemeMedia.ThemeSongsResult.Items).ToArray()
            };
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
