using System;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface ISettingsPage
    /// </summary>
    public interface ISettingsPage
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        Uri ThumbUri { get; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        Page GetPage();

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        SettingsPageCategory Category { get; }
    }

    /// <summary>
    /// Interface ISystemSettingsPage
    /// </summary>
    public interface IOrderedSettingsPage : ISettingsPage
    {
        int Order { get; }
    }

    public enum SettingsPageCategory
    {
        System,
        Plugin
    }
}
