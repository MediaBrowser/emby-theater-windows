
namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IFullscreenVideoPage
    /// </summary>
    public interface IFullscreenVideoPage
    {
        /// <summary>
        /// Toggles the OSD
        /// </summary>
        void ToggleOsd();

        /// <summary>
        /// Shows the on screen display.
        /// </summary>
        void ShowOsd();

        /// <summary>
        /// Hides the  on screen display.
        /// </summary>
        void HideOsd();

        /// <summary>
        /// Toggles the info panel.
        /// </summary>
        void ToggleInfoPanel();

        /// <summary>
        /// Shows the info panel.
        /// </summary>
        void ShowInfoPanel();

        /// <summary>
        /// Hides info panel.
        /// </summary>
        void HideInfoPanel();

    }
}
