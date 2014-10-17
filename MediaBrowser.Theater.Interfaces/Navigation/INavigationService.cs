using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
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
        /// Gets the current page.
        /// </summary>
        /// <value>The current page.</value>
        Page CurrentPage { get; }

        /// <summary>
        /// Navigates to home page.
        /// </summary>
        /// <returns>Task.</returns>
        Task NavigateToHomePage();

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        Task Navigate(FrameworkElement page);

        /// <summary>
        /// Navigates to settings page.
        /// </summary>
        Task NavigateToSettingsPage();

        /// <summary>
        /// Navigates to server selection.
        /// </summary>
        /// <returns>Task.</returns>
        Task NavigateToServerSelection();

        /// <summary>
        /// Navigates to connect login.
        /// </summary>
        /// <returns>Task.</returns>
        Task NavigateToConnectLogin();
        
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
        Task NavigateToItem(BaseItemDto item, ViewType context = ViewType.Folders);

        /// <summary>
        /// Navigates to person.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="mediaItemId">The media item id.</param>
        /// <returns>Task.</returns>
        Task NavigateToPerson(string name, ViewType context = ViewType.Folders, string mediaItemId = null);

        /// <summary>
        /// Navigates to Genre in either a Move, Tv or Game view.
        /// </summary>
        /// <param name="genre">The Genre.</param>
        /// <param name="context">The context - Move, TV or Game</param>
        /// <returns>Task.</returns>
        Task NavigateToGenre(string genre, ViewType context);
      
        /// <summary>
        /// Navigates to the search page
        /// </summary>
        Task NavigateToSearchPage();

        /// <summary>
        /// Navigates the back.
        /// </summary>
        Task NavigateBack();

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        Task NavigateForward();

        /// <summary>
        /// Navigates to image viewer.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns>Task.</returns>
        Task NavigateToImageViewer(ImageViewerViewModel viewModel);

        /// <summary>
        /// Navigates to the modal behind the home page
        /// </summary>
        /// <returns></returns>
        void NavigateToBackModal();

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
