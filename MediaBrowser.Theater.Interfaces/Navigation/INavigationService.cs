using MediaBrowser.Model.Dto;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Navigation
{
    /// <summary>
    /// Interface INavigationService
    /// </summary>
    public interface INavigationService
    {
        event EventHandler<NavigationEventArgs> Navigated;

        /// <summary>
        /// Navigates to home page.
        /// </summary>
        /// <returns>Task.</returns>
        Task NavigateToHomePage(string userId);

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
        Task NavigateToItem(BaseItemDto item, string context = null);

        /// <summary>
        /// Navigates to person.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        Task NavigateToPerson(string name, string context = null);

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

        /// <summary>
        /// Removes the pages from history.
        /// </summary>
        /// <param name="count">The count.</param>
        void RemovePagesFromHistory(int count);
    }
}
