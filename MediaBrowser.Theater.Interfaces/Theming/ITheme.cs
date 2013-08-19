using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Theming
{
    /// <summary>
    /// Interface ITheme
    /// </summary>
    public interface ITheme
    {
        /// <summary>
        /// Gets the item page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        Page GetItemPage(BaseItemDto item, string context);

        /// <summary>
        /// Gets the person page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        Page GetPersonPage(BaseItemDto item, string context);
        
        /// <summary>
        /// Shows the default error message.
        /// </summary>
        void ShowDefaultErrorMessage();

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>MessageBoxResult.</returns>
        MessageBoxResult ShowMessage(MessageBoxInfo options);

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="title">The title.</param>
        void SetPageTitle(string title);

        /// <summary>
        /// Sets the default page title.
        /// </summary>
        void SetDefaultPageTitle();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        void Load();

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        void Unload();

        /// <summary>
        /// Sets the global content visibility.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        void SetGlobalContentVisibility(bool visible);

        /// <summary>
        /// Gets the default name of the home page.
        /// </summary>
        /// <value>The default name of the home page.</value>
        string DefaultHomePageName { get; }
    }
}
