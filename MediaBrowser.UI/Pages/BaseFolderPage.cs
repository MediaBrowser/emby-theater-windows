using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages
{
    /// <summary>
    /// Provides a base class for pages based on a folder item (list, home)
    /// </summary>
    public abstract class BaseFolderPage : BasePage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFolderPage" /> class.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        protected BaseFolderPage(string itemId)
            : base()
        {
            ItemId = itemId;
        }

        /// <summary>
        /// The _item id
        /// </summary>
        private string _itemId;
        /// <summary>
        /// Gets or sets the Id of the item being displayed
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
        /// The _sort by
        /// </summary>
        private string _sortBy;
        /// <summary>
        /// Gets or sets the name of the current sort function
        /// </summary>
        /// <value>The sort by.</value>
        public string SortBy
        {
            get { return _sortBy; }
            private set
            {
                _sortBy = value;
                OnPropertyChanged("SortBy");
            }
        }

        /// <summary>
        /// The _folder
        /// </summary>
        private BaseItemDto _folder;
        /// <summary>
        /// Gets or sets the Folder being displayed
        /// </summary>
        /// <value>The folder.</value>
        public BaseItemDto Folder
        {
            get { return _folder; }

            set
            {
                _folder = value;
                OnPropertyChanged("Folder");
                OnFolderChanged();
            }
        }

        /// <summary>
        /// If wrap panels are being used this will get the orientation that should be used, based on scroll direction
        /// </summary>
        /// <value>The wrap panel orientation.</value>
        public Orientation WrapPanelOrientation
        {
            get
            {
                return DisplayPreferences.ScrollDirection == ScrollDirection.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
            }
        }

        /// <summary>
        /// The _display preferences
        /// </summary>
        private DisplayPreferences _displayPreferences;
        /// <summary>
        /// Gets of sets the current DisplayPreferences
        /// </summary>
        /// <value>The display preferences.</value>
        public DisplayPreferences DisplayPreferences
        {
            get { return _displayPreferences; }

            private set
            {
                _displayPreferences = value;

                NotifyDisplayPreferencesChanged();
            }
        }

        /// <summary>
        /// The _children
        /// </summary>
        private ItemsResult _children;
        /// <summary>
        /// Gets or sets the children of the Folder being displayed
        /// </summary>
        /// <value>The children.</value>
        public ItemsResult Children
        {
            get { return _children; }

            private set
            {
                _children = value;
                OnPropertyChanged("Children");
                ChildCount = _children.TotalRecordCount;
                OnChildrenChanged();

            }
        }

        /// <summary>
        /// The _display children
        /// </summary>
        private ObservableCollection<DtoBaseItemViewModel> _displayChildren;
        /// <summary>
        /// Gets the actual children that should be displayed.
        /// Subclasses should bind to this, not Children.
        /// </summary>
        /// <value>The display children.</value>
        public ObservableCollection<DtoBaseItemViewModel> DisplayChildren
        {
            get { return _displayChildren; }

            private set
            {
                _displayChildren = value;
                OnPropertyChanged("DisplayChildren");
            }
        }

        /// <summary>
        /// The _child count
        /// </summary>
        private int _childCount;
        /// <summary>
        /// Gets or sets the number of children within the Folder
        /// </summary>
        /// <value>The child count.</value>
        public int ChildCount
        {
            get { return _childCount; }

            private set
            {
                _childCount = value;
                OnPropertyChanged("ChildCount");
            }
        }

        /// <summary>
        /// The _average primary image aspect ratio
        /// </summary>
        private double _averagePrimaryImageAspectRatio;
        /// <summary>
        /// Gets or sets the average primary image aspect ratio for all items
        /// </summary>
        /// <value>The average primary image aspect ratio.</value>
        public double AveragePrimaryImageAspectRatio
        {
            get { return _averagePrimaryImageAspectRatio; }

            private set
            {
                _averagePrimaryImageAspectRatio = value;
                OnPropertyChanged("AveragePrimaryImageAspectRatio");
            }
        }

        /// <summary>
        /// Gets the aspect ratio that should be used based on a given ImageType
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <returns>System.Double.</returns>
        public double GetAspectRatio(ImageType imageType)
        {
            return GetAspectRatio(imageType, AveragePrimaryImageAspectRatio);
        }

        /// <summary>
        /// Gets the aspect ratio that should be used based on a given ImageType
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="averagePrimaryImageAspectRatio">The average primary image aspect ratio.</param>
        /// <returns>System.Double.</returns>
        public static double GetAspectRatio(ImageType imageType, double averagePrimaryImageAspectRatio)
        {
            switch (imageType)
            {
                case ImageType.Art:
                    return 1.777777777777778;
                case ImageType.Backdrop:
                    return 1.777777777777778;
                case ImageType.Banner:
                    return 5.414285714285714;
                case ImageType.Disc:
                    return 1;
                case ImageType.Logo:
                    return 1.777777777777778;
                case ImageType.Primary:
                    return averagePrimaryImageAspectRatio;
                case ImageType.Thumb:
                    return 1.777777777777778;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public async override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            // Reload the Folder when the itemId changes
            if (name.Equals("ItemId"))
            {
                await ReloadFolder();
            }
        }

        /// <summary>
        /// Reloads the folder
        /// </summary>
        /// <returns>Task.</returns>
        private async Task ReloadFolder()
        {
            try
            {
                if (string.IsNullOrEmpty(ItemId))
                {
                    Folder = await App.Instance.ApiClient.GetRootFolderAsync(App.Instance.CurrentUser.Id);
                }
                else
                {
                    Folder = await App.Instance.ApiClient.GetItemAsync(ItemId, App.Instance.CurrentUser.Id);
                }
            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Gets called anytime the Folder gets refreshed
        /// </summary>
        protected virtual async Task OnFolderChanged()
        {
            SetBackdrops(Folder);

            DisplayPreferences = await App.Instance.ApiClient.GetDisplayPreferencesAsync(Folder.DisplayPreferencesId);

            if (DisplayPreferences.RememberSorting)
            {
                SortBy = DisplayPreferences.SortBy;
            }

            // Default to SortName
            if (string.IsNullOrEmpty(SortBy))
            {
                SortBy = ItemSortBy.SortName;
            }

            await ReloadChildren();
        }

        /// <summary>
        /// Gets called anytime the Children get refreshed
        /// </summary>
        protected virtual void OnChildrenChanged()
        {
            AveragePrimaryImageAspectRatio = DtoBaseItemViewModel.GetAveragePrimaryImageAspectRatio(Children.Items);

            if (DisplayPreferences != null)
            {
                DisplayPreferences.PrimaryImageWidth = Convert.ToInt32(DisplayPreferences.PrimaryImageHeight * GetAspectRatio(PreferredImageType));
            }

            DisplayChildren = DtoBaseItemViewModel.GetObservableItems(Children.Items, AveragePrimaryImageAspectRatio);
            
            NotifyDisplayPreferencesChanged();
        }

        protected virtual ImageType PreferredImageType
        {
            get { return ImageType.Primary; }
        }

        /// <summary>
        /// Reloads the Folder's children
        /// </summary>
        /// <returns>Task.</returns>
        public async Task ReloadChildren()
        {
            var query = new ItemQuery
            {
                ParentId = Folder.Id,

                Fields = new[] {
                                 ItemFields.UserData,
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated,
                                 ItemFields.MediaStreams,
                                 ItemFields.Taglines,
                                 ItemFields.Genres,
                                 ItemFields.SeriesInfo,
                                 ItemFields.Overview
                             },

                UserId = App.Instance.CurrentUser.Id
            };

            if (!string.IsNullOrEmpty(SortBy))
            {
                query.SortBy = new[] { SortBy };
            }

            try
            {
                Children = await App.Instance.ApiClient.GetItemsAsync(query);
            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Gets called anytime a DisplayPreferences property is updated
        /// </summary>
        public virtual void NotifyDisplayPreferencesChanged()
        {
            OnPropertyChanged("DisplayPreferences");

            if (DisplayChildren != null)
            {
                // Notify all of the child view models
                foreach (var child in DisplayChildren)
                {
                    child.AveragePrimaryImageAspectRatio = AveragePrimaryImageAspectRatio;
                    child.ImageHeight = DisplayPreferences.PrimaryImageHeight;
                    child.ImageWidth = DisplayPreferences.PrimaryImageWidth;
                    child.ViewType = DisplayPreferences.ViewType;
                }
            }

            OnPropertyChanged("WrapPanelOrientation");
        }

        /// <summary>
        /// Changes the sort option on the page
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>Task.</returns>
        public async Task UpdateSortOption(string option)
        {
            var tasks = new List<Task>();

            SortBy = option;

            if (DisplayPreferences.RememberSorting)
            {
                DisplayPreferences.SortBy = option;
                NotifyDisplayPreferencesChanged();

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await App.Instance.ApiClient.UpdateDisplayPreferencesAsync(DisplayPreferences);
                    }
                    catch
                    {
                        App.Instance.ShowDefaultErrorMessage();
                    }
                }));
            }

            tasks.Add(ReloadChildren());

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Updates the remember sort.
        /// </summary>
        /// <param name="remember">if set to <c>true</c> [remember].</param>
        /// <returns>Task.</returns>
        public async Task UpdateRememberSort(bool remember)
        {
            DisplayPreferences.RememberSorting = remember;

            if (remember)
            {
                DisplayPreferences.SortBy = SortBy;
            }

            await App.Instance.ApiClient.UpdateDisplayPreferencesAsync(DisplayPreferences);
        }
    }
}
