using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected override Task<ItemsResult> GetItemsAsync()
        {
            return Task.FromResult(new ItemsResult
            {
                TotalRecordCount = _item.People.Length,

                Items = _item.People.Select(i => new BaseItemDto
                {
                    Name = i.Name,

                    Overview = i.Role,
                    Path = i.Type,

                    ImageTags = i.PrimaryImageTag.HasValue ? new[] { i.PrimaryImageTag.Value }.ToDictionary(p => ImageType.Primary) : new Dictionary<ImageType, Guid>(),

                    Type = "Person"

                }).ToArray()
            });
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

            vm.PersonRole = item.Overview;
            vm.PersonType = item.Path;

            return vm;
        }

        protected override double GetImageDisplayHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (medianPrimaryImageAspectRatio.Equals(1))
            {
                medianPrimaryImageAspectRatio = .6666666666666667;
            }

            double height = displayPreferences.PrimaryImageWidth;

            return height / medianPrimaryImageAspectRatio;
        }
    }
}
