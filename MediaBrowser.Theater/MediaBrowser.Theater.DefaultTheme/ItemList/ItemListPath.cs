using System.Threading.Tasks;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.ItemList
{
    /// <summary>
    ///     The path to a page for displaying the results of an items query.
    /// </summary>
    public class ItemListPath : NavigationPathArg<ItemListParameters> { }

    public static class ItemsListPathExtensions
    {
        /// <summary>
        ///     Gets a path to the a page to display the specified items result.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="parameters">The items to display.</param>
        /// <returns>A path to the a page to display the specified items result.</returns>
        public static ItemListPath ItemList(this Go go, ItemListParameters parameters)
        {
            return new ItemListPath { Parameter = parameters };
        }
    }

    #region Parameters

    public class ItemListParameters
    {
        public Task<ItemsResult> Items { get; set; }
        public string Title { get; set; }
    }

    #endregion
}