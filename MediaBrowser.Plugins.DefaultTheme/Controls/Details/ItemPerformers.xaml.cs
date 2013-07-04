using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.ObjectModel;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Interaction logic for ItemPerformers.xaml
    /// </summary>
    public partial class ItemPerformers : BaseDetailsControl
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPerformers" /> class.
        /// </summary>
        public ItemPerformers(IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _sessionManager = sessionManager;
            InitializeComponent();
        }

        /// <summary>
        /// The _itemsResult
        /// </summary>
        private ItemsResult _itemsResult;
        /// <summary>
        /// Gets or sets the children of the Folder being displayed
        /// </summary>
        /// <value>The children.</value>
        public ItemsResult ItemsResult
        {
            get { return _itemsResult; }

            private set
            {
                _itemsResult = value;
                OnPropertyChanged("ItemsResult");

                //Items = DtoBaseItemViewModel.GetObservableItems(ItemsResult.Items, _apiClient, _imageManager);

                //double width = 300;
                //double height = 300;

                //if (Items.Count > 0)
                //{
                //    height = width / Items[0].MedianPrimaryImageAspectRatio;
                //}

                //foreach (var item in Items)
                //{
                //    item.ImageDisplayWidth = width;
                //    item.ImageHeight = height;
                //}
            }
        }

        /// <summary>
        /// The _display children
        /// </summary>
        private ObservableCollection<BaseItemDtoViewModel> _items;
        /// <summary>
        /// Gets the actual children that should be displayed.
        /// Subclasses should bind to this, not ItemsResult.
        /// </summary>
        /// <value>The display children.</value>
        public ObservableCollection<BaseItemDtoViewModel> Items
        {
            get { return _items; }

            private set
            {
                _items = value;
                lstItems.ItemsSource = value;
                OnPropertyChanged("Items");
            }
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected override async void OnItemChanged()
        {
            ItemsResult = await _apiClient.GetPeopleAsync(new PersonsQuery
            {
                ParentId = Item.Id,
                UserId = _sessionManager.CurrentUser.Id,
                Fields = new[] { ItemFields.PrimaryImageAspectRatio }
            });
        }
    }
}
