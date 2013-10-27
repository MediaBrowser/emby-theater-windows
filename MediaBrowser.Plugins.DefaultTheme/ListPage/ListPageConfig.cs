using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.ListPage
{
    public class ListPageConfig
    {
        public List<TabItem> IndexOptions;
        public int PosterImageWidth = 210;
        public int ThumbImageWidth = 600;
        public int ListImageWidth = 160;

        public string DefaultViewType = ListViewTypes.Poster;

        public string PageTitle;
        public Dictionary<string, string> SortOptions;

        public ViewType Context;

        public Func<ItemListViewModel, DisplayPreferences, Task<ItemsResult>> CustomItemQuery;
        
        public ListPageConfig()
        {
            IndexOptions = new List<TabItem>();

            SortOptions = new Dictionary<string, string>();
        }
    }
}
