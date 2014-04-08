using System;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Screensavers
{
    /// <summary>
    /// base logic for screen savers
    /// </summary>
    public class ScreensaverWindowBase : BaseModalWindow, IScreensaver
    {
        protected readonly IPresentationManager _presentationManager;
        protected readonly IScreensaverManager _screensaverManager;
        protected readonly ILogger _logger;

        public ScreensaverWindowBase(IPresentationManager presentationManager, IScreensaverManager screensaverManager, ILogger logger)
        {
            _presentationManager = presentationManager;
            _screensaverManager = screensaverManager;
            _logger = logger;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _screensaverManager.StopScreenSaver(); // re-entrant into clase -  back in to closemodal
        }

        /// <summary>
        /// The _last mouse move point
        /// </summary>
        private Point? _lastMouseMovePoint;

        /// <summary>
        /// Handles OnMouseMove to auto-select the item that's being moused over
        /// </summary>
        /// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseEventArgs" />.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Store the last position for comparison purposes
            // Even if the mouse is not moving this event will fire as elements are showing and hiding
            var pos = e.GetPosition(this);

            if (!_lastMouseMovePoint.HasValue)
            {
                _lastMouseMovePoint = pos;
                return;
            }

            if (pos == _lastMouseMovePoint.Value)
            {
                return;
            }
            _logger.Debug("OnMouseMove {0} {1}", pos, _lastMouseMovePoint.Value);
            _screensaverManager.StopScreenSaver(); // re-entrant into clase -  back in to closemodal
        }
       

        void IScreensaver.ShowModal()
        {
            ShowModal(_presentationManager.Window);
        }

        void IScreensaver.Close()
        {
            CloseModal();
        }
    }
}
