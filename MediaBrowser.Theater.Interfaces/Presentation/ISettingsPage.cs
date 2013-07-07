using MediaBrowser.Model.Dto;
using System;

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
        /// Gets the type of the page.
        /// </summary>
        /// <value>The type of the page.</value>
        Type PageType { get; }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        SettingsPageCategory Category { get; }

        /// <summary>
        /// Determines whether the specified user is visible.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the specified user is visible; otherwise, <c>false</c>.</returns>
        bool IsVisible(UserDto user);
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
