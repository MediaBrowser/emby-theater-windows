using System;
using System.Linq;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using System.Threading.Tasks;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for People.xaml
    /// </summary>
    public partial class People : BaseItemsControl
    {
        private readonly BaseItemDto _item;

        public People(Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow, BaseItemDto item) 
            : base(displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            _item = item;
            InitializeComponent();
        }

        protected override ExtendedListBox ItemsList
        {
            get { return LstItems; }
        }

        protected override async Task<ItemsResult> GetItemsAsync()
        {
            try
            {
                return await ApiClient.GetPeopleAsync(new PersonsQuery
                {
                    ParentId = _item.Id,
                    UserId = SessionManager.CurrentUser.Id,
                    Fields = new[] { ItemFields.PrimaryImageAspectRatio }
                });

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

            var roles = _item.People
                .Where(i => string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                .Select(i => string.IsNullOrEmpty(i.Role) ? i.Type : i.Role)
                .ToArray();

            vm.PersonRole = string.Join(",", roles);

            return vm;
        }

        protected override double GetImageDisplayHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (medianPrimaryImageAspectRatio.Equals(1))
            {
                medianPrimaryImageAspectRatio = .6666666666666667;
            }

            double height = displayPreferences.PrimaryImageWidth;

            return height/medianPrimaryImageAspectRatio;
        }
    }
}
