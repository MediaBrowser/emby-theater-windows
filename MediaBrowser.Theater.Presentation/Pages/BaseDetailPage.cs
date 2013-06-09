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
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        protected IApiClient ApiClient { get; private set; }
        /// <summary>
        /// Gets the session manager.
        /// </summary>
        /// <value>The session manager.</value>
        protected ISessionManager SessionManager { get; private set; }
        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        protected IApplicationWindow ApplicationWindow { get; private set; }
        /// <summary>
        /// Gets the theme manager.
        /// </summary>
        /// <value>The theme manager.</value>
        protected IThemeManager ThemeManager { get; private set; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="BaseDetailPage" /> class.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="applicationWindow">The application window.</param>
        /// <param name="themeManager">The theme manager.</param>
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
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
