using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Collections.Generic;
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
        /// Gets the folder page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="displayPreferences">The display preferences.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        Page GetFolderPage(BaseItemDto item, DisplayPreferences displayPreferences, string context);
        
        /// <summary>
        /// Gets the person page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        Page GetPersonPage(BaseItemDto item, string context);

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
        /// Gets the default name of the home page.
        /// </summary>
        /// <value>The default name of the home page.</value>
        string DefaultHomePageName { get; }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns>List{ResourceDictionary}.</returns>
        IEnumerable<ResourceDictionary> GetResources();

        /// <summary>
        /// Gets the page content view model.
        /// </summary>
        /// <returns>PageContentViewModel.</returns>
        PageContentViewModel CreatePageContentDataContext();
    }
}
