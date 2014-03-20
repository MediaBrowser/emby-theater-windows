using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Key = System.Windows.Input.Key;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class AlphaInputViewModel : ItemListViewModel
    {
        private readonly Func<AlphaInputViewModel, Task<Boolean>> _alphaItemSelectedDelegate;
        public ICommand ClickCommand { get; private set; }

        public AlphaInputViewModel(Func<AlphaInputViewModel, Task<Boolean>> alphaItemSelectedDelegate, IPresentationManager presentationManager, IImageManager imageManager, IApiClient apiClient, INavigationService navigationService, IPlaybackManager playbackManager, ILogger logger, IServerEvents serverEvents)
            : base(null, presentationManager, imageManager, apiClient, navigationService, playbackManager, logger, serverEvents)
        {
           _alphaItemSelectedDelegate = alphaItemSelectedDelegate;     // SWA TODO should proably set up a event for this

            EnableBackdropsForCurrentItem = false;
            EnableBackdropsForCurrentItem = false;
            ScrollDirection = ScrollDirection.Horizontal;
            ShowSidebar = false;
            AutoSelectFirstItem = false;
            ShowLoadingAnimation = false;
            IsVirtualizationRequired = false;

            ClickCommand = new RelayCommand(Click);

            IndexOptionsCollectionView.CurrentChanged -= IndexOptionsCollectionView_CurrentChanged;
            AddIndexOptions(AlphabetTabItemList);
        }
     

        private Boolean isHandledKey(System.Windows.Input.Key key)
        {
            return key == Key.Delete || key == Key.Space || (key >= Key.A && key <= Key.Z);
        }

        public void UserInput_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e, ExtendedListBox alphabetMenu)
        {
            if (isHandledKey(e.Key))
            {
                string KeyVal;

                if (e.Key == Key.Delete)
                {
                    KeyVal = "back";
                }
                else if (e.Key == Key.Space)
                {
                    KeyVal = "_";
                }
                else
                {
                    KeyVal = e.Key.ToString().ToLower();
                }

                // TODO - move hightlight code to AlphabetItemViewList viewmodel
                var item = AlphabetTabItemList.Find(t => t.DisplayName == KeyVal);
                var currentItem = IndexOptionsCollectionView.CurrentItem as MediaBrowser.Theater.Presentation.ViewModels.TabItem;
                var alReadySelected = (item != null) && (currentItem != null) && item.DisplayName == currentItem.DisplayName;

                var listBoxItem = alphabetMenu.ItemContainerGenerator.ContainerFromItem(item) as ExtendedListBoxItem;
                if (listBoxItem != null)
                {
                    if (alReadySelected)
                        listBoxItem.IsSelected = false;

                    listBoxItem.IsSelected = true;
                    OnIndexSelectionChange(null);
                }
               e.Handled = true;
            }
        }

        private async void Click(object commandParameter)
        {
            var item = commandParameter as ItemViewModel;
            
           CurrentIndexOption = IndexOptionsCollectionView.CurrentItem as TabItem;  // should already have changed because of focus change
           OnIndexSelectionChange(null);
           
        }

        public new void OnIndexSelectionChange(object state)
        {
            IndexOptionsSelectionChange();
        }

        public void IndexOptionsSelectionChange()
        {
            CurrentIndexOption = IndexOptionsCollectionView.CurrentItem as TabItem;
           _alphaItemSelectedDelegate(this);
         }

        private List<TabItem> _alphabetTabItemList = null;
        public List<TabItem> AlphabetTabItemList
        {
            get
            {
                if (_alphabetTabItemList == null)
                {
                    _alphabetTabItemList = new List<TabItem>();

                    var chars = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "_", "back" };

                    _alphabetTabItemList.AddRange(chars.Select(i => new TabItem { Name = i, DisplayName = i, TabType = "Alphabet" }));

                    return _alphabetTabItemList;
                }
                else
                {
                    return _alphabetTabItemList;
                }
            }
        }
    } 
}