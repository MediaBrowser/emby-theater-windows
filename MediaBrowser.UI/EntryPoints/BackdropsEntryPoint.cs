using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.UI.EntryPoints
{
    /// <summary>
    /// Class BackdropsEntryPoint
    /// </summary>
    class BackdropsEntryPoint : IStartupEntryPoint, IDisposable
    {
        /// <summary>
        /// The _nav
        /// </summary>
        private readonly INavigationService _nav;
        /// <summary>
        /// The _presentation manager
        /// </summary>
        private readonly IPresentationManager _presentationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackdropsEntryPoint"/> class.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <param name="presentationManager">The presentation manager.</param>
        public BackdropsEntryPoint(INavigationService nav, IPresentationManager presentationManager)
        {
            _nav = nav;
            _presentationManager = presentationManager;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            _nav.Navigated += _nav_Navigated;
        }

        /// <summary>
        /// Handles the Navigated event of the _nav control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NavigationEventArgs"/> instance containing the event data.</param>
        void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            var page = e.NewPage;

            if (!(page is ISupportsBackdrops))
            {
                _presentationManager.ClearBackdrops();
                return;
            }

            var itemBackdrops = page as ISupportsItemBackdrops;

            if (itemBackdrops != null)
            {
                _presentationManager.SetBackdrops(itemBackdrops.BackdropItem);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _nav.Navigated -= _nav_Navigated;
        }
    }
}
