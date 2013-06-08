using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Pages
{
    /// <summary>
    /// Provides a base class for detail pages
    /// </summary>
    public abstract class BaseDetailPage : Page, INotifyPropertyChanged
    {
        protected IApiClient ApiClient { get; private set; }
        protected ISessionManager SessionManager { get; private set; }
        protected IApplicationWindow ApplicationWindow { get; private set; }
        protected IThemeManager ThemeManager { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The _item id
        /// </summary>
        private string _itemId;
        /// <summary>
        /// Gets or sets the id of the item being displayed
        /// </summary>
        /// <value>The item id.</value>
        protected string ItemId
        {
            get { return _itemId; }
            private set
            {
                _itemId = value;
                OnPropertyChanged("ItemId");
            }
        }

        /// <summary>
        /// The _item
        /// </summary>
        private BaseItemDto _item;
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                _item = value;
                OnPropertyChanged("Item");
                OnItemChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can resume.
        /// </summary>
        /// <value><c>true</c> if this instance can resume; otherwise, <c>false</c>.</value>
        protected bool CanResume
        {
            get { return Item.CanResume; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can queue.
        /// </summary>
        /// <value><c>true</c> if this instance can queue; otherwise, <c>false</c>.</value>
        protected bool CanQueue
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can play trailer.
        /// </summary>
        /// <value><c>true</c> if this instance can play trailer; otherwise, <c>false</c>.</value>
        protected bool CanPlayTrailer
        {
            get { return Item.HasTrailer; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDetailPage" /> class.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="apiClient">The API client.</param>
        protected BaseDetailPage(string itemId, IApiClient apiClient, ISessionManager sessionManager, IApplicationWindow applicationWindow, IThemeManager themeManager)
            : base()
        {
            ThemeManager = themeManager;
            ApplicationWindow = applicationWindow;
            SessionManager = sessionManager;
            ApiClient = apiClient;
            ItemId = itemId;
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public async void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            // Reload the item when the itemId changes
            if (name.Equals("ItemId"))
            {
                await ReloadItem();
            }
        }

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task ReloadItem()
        {
            try
            {
                Item = await ApiClient.GetItemAsync(ItemId, SessionManager.CurrentUser.Id);
            }
            catch (HttpException)
            {
                ThemeManager.CurrentTheme.ShowDefaultErrorMessage();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = this;
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected virtual void OnItemChanged()
        {
            ApplicationWindow.SetBackdrops(Item);
        }
    }
}
