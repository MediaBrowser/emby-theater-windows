using MediaBrowser.Model.Dto;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Interfaces.Navigation
{
    /// <summary>
    /// Interface INavigationService
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to home page.
        /// </summary>
        DispatcherOperation NavigateToHomePage(BaseItemDto rootItem);

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        DispatcherOperation Navigate(Page page);

        /// <summary>
        /// Navigates to settings page.
        /// </summary>
        DispatcherOperation NavigateToSettingsPage();

        /// <summary>
        /// Navigates to login page.
        /// </summary>
        DispatcherOperation NavigateToLoginPage();

        /// <summary>
        /// Navigates to internal player page.
        /// </summary>
        DispatcherOperation NavigateToInternalPlayerPage();

        /// <summary>
        /// Navigates to item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        Task NavigateToItem(BaseItemDto item, string context);

        /// <summary>
        /// Navigates the back.
        /// </summary>
        DispatcherOperation NavigateBack();

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        DispatcherOperation NavigateForward();
    }
}
