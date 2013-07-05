using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public abstract class BaseFolderControl : BaseItemsControl
    {
        /// <summary>
        /// The _parent item
        /// </summary>
        private BaseItemDto _parentItem;
        /// <summary>
        /// Gets or sets the parent item.
        /// </summary>
        /// <value>The parent item.</value>
        public BaseItemDto ParentItem
        {
            get { return _parentItem; }

            set
            {
                _parentItem = value;
                OnPropertyChanged("ParentItem");
            }
        }

        protected BaseFolderControl(BaseItemDto parentItem, DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow)
            : base(displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            _parentItem = parentItem;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            OnPropertyChanged("ParentItem");
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (string.Equals("ParentItem", name))
            {
                OnParentItemChanged();
            }
        }

        /// <summary>
        /// Called when [parent item changed].
        /// </summary>
        protected virtual void OnParentItemChanged()
        {
        }
    }
}
