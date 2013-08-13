using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Windows;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class AppViewModel
    /// </summary>
    public class AppViewModel : BaseViewModel
    {
        /// <summary>
        /// The _presentation
        /// </summary>
        private readonly IPresentationManager _presentation;
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _app == null ? null : _app.Name; }
        }

        /// <summary>
        /// The _app
        /// </summary>
        private ITheaterApp _app;
        /// <summary>
        /// Gets or sets the app.
        /// </summary>
        /// <value>The app.</value>
        public ITheaterApp App
        {
            get { return _app; }

            set
            {
                var changed = _app != value;

                _app = value;

                if (changed)
                {
                    OnPropertyChanged("App");
                    OnPropertyChanged("Name");

                    ReloadTileImage();
                }
            }
        }

        /// <summary>
        /// The _tile image
        /// </summary>
        private FrameworkElement _tileImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppViewModel"/> class.
        /// </summary>
        /// <param name="presentation">The presentation.</param>
        /// <param name="logger">The logger.</param>
        public AppViewModel(IPresentationManager presentation, ILogger logger)
        {
            _presentation = presentation;
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the tile image.
        /// </summary>
        /// <value>The tile image.</value>
        public FrameworkElement TileImage
        {
            get { return _tileImage; }

            set
            {
                var changed = !Equals(_tileImage, value);

                _tileImage = value;

                if (changed)
                {
                    OnPropertyChanged("TileImage");
                }
            }
        }

        /// <summary>
        /// Reloads the tile image.
        /// </summary>
        public void ReloadTileImage()
        {
            try
            {
                TileImage = App.GetTileImage();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting image from app {0}", ex, _app.Name);

                _presentation.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OK,
                    Caption = "Error",
                    Icon = MessageBoxIcon.Error,
                    Text = ex.Message
                });
            }
        }

        /// <summary>
        /// Launches this instance.
        /// </summary>
        public async void Launch()
        {
            try
            {
                await _app.Launch();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error launching app {0}", ex, _app.Name);

                _presentation.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OK,
                    Caption = "Error",
                    Icon = MessageBoxIcon.Error,
                    Text = ex.Message
                });
            }
        }
    }
}
