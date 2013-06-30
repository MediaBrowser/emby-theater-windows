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
        Task NavigateToHomePage(BaseItemDto rootItem);

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        Task Navigate(Page page);

        /// <summary>
        /// Navigates to settings page.
        /// </summary>
        Task NavigateToSettingsPage();

        /// <summary>
        /// Navigates to login page.
        /// </summary>
        Task NavigateToLoginPage();

        /// <summary>
        /// Navigates to internal player page.
        /// </summary>
        Task NavigateToInternalPlayerPage();

        /// <summary>
        /// Navigates to item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        Task NavigateToItem(BaseItemDto item, string context);

        /// <summary>
        /// Navigates the back.
        /// </summary>
        Task NavigateBack();

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        Task NavigateForward();

        /// <summary>
        /// Clears the history.
        /// </summary>
        void ClearHistory();
    }
}
