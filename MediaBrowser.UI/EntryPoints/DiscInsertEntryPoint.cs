using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using MediaBrowser.Theater.Interfaces.Theming;

namespace MediaBrowser.UI.EntryPoints
{
    /// <summary>
    /// Class DiskInsertEntryPoint
    /// </summary>
    class DiskInsertEntryPoint : IStartupEntryPoint, IAppFactory, IApp, IDisposable
    {
       
        private readonly INavigationService _nav;
        private readonly IPresentationManager _presentationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackdropsEntryPoint"/> class.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <param name="presentationManager">The presentation manager.</param>
        public DiskInsertEntryPoint(INavigationService nav, IPresentationManager presentationManager)
        {
            _nav = nav;
            _presentationManager = presentationManager;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
        }

        /// <summary>
        /// IAppFactory.IAppFactory
        /// </summary>
        public IEnumerable<IApp> GetApps()
        {
            return new List<IApp>() { this };
        }

        /// <summary>
        ///  IApp.Name 
        ///  Gets the name.
        ///  </summary>
        /// <value>The name.</value>
        public string Name { get { return "Disk Insert"; } }

        /// <summary>
        /// IApp.Launch
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Task Launch() {

           var msgResult = _presentationManager.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OKCancel,
                Caption = "Insert disc",
                Icon = MessageBoxIcon.Warning,
                Text = "Insert the disk into the cd/dvd player and ok when ready "
            });

            //if (msgResult == MessageBoxResult.OK)
            return null;
        
        }

        /// <summary>
        /// IApp.Thumb
        /// Gets the thumb image.
        /// </summary>
        /// <returns>FrameworkElement.</returns>
        public FrameworkElement GetThumbImage() { return null; }



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
