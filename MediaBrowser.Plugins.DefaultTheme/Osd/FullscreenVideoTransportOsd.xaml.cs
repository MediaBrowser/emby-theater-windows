using System.Diagnostics;
using MediaBrowser.Model.Dto;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.Osd
{
    /// <summary>
    /// Interaction logic for FullscreenVideoTransportOsd.xaml
    /// </summary>
    public partial class FullscreenVideoTransportOsd : UserControl
    {
        /// <summary>
        /// The is position slider dragging
        /// </summary>
        private bool _isPositionSliderUpdating;

        /// <summary>
        /// Event handler for mouse down, add via addhandler so we can catch handled events
        /// </summary>
        private MouseButtonEventHandler _previewMouseDown;

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public TransportOsdViewModel ViewModel
        {
            get { return DataContext as TransportOsdViewModel; }
        }

        public FullscreenVideoTransportOsd()
        {
            InitializeComponent();

            DataContextChanged += FullscreenVideoTransportOsd_DataContextChanged;
            Loaded += FullscreenVideoTransportOsd_Loaded;
            Unloaded += FullscreenVideoTransportOsd_Unloaded;
        }

        private void FullscreenVideoTransportOsd_Loaded(object sender, RoutedEventArgs e)
        {
            // The silder has IsMoveToPointEnabled=true, which means it moves the thumb when we click the mouse, which also set handled to true
            // so we can't get the event. We need to turn event on so we can tell the slider to ignore position tick property notifications until
            // we get a mouse up and update the slider position. So we use AddHanlder, so we can catch handled events
            if (_previewMouseDown == null)
            {
                _previewMouseDown = new MouseButtonEventHandler(CurrentPositionSlider_PreviewMouseDown);
                CurrentPositionSlider.AddHandler(PreviewMouseDownEvent, _previewMouseDown, true);
            }
        }

        private void FullscreenVideoTransportOsd_Unloaded(object sender, RoutedEventArgs e)
        {
            CurrentPositionSlider.RemoveHandler(PreviewMouseDownEvent, _previewMouseDown);
            _previewMouseDown = null;

            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= vm_PropertyChanged;
            }
        }

        private void FullscreenVideoTransportOsd_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged += vm_PropertyChanged;

                TxtNowPlayingName.Text = NowPlayingInfo.GetName(vm.NowPlayingItem);
                UpdateLogo(vm, vm.NowPlayingItem);
            }
        }

        private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            if (string.Equals(e.PropertyName, "NowPlayingItem"))
            {
                TxtNowPlayingName.Text = NowPlayingInfo.GetName(vm.NowPlayingItem);
                UpdateLogo(vm, vm.NowPlayingItem);
            }
            else if (string.Equals(e.PropertyName, "PositionTicks"))
            {
                if (!_isPositionSliderUpdating)
                {
                    CurrentPositionSlider.Value = vm.PositionTicks;
                }
            }
        }

        private async void UpdateLogo(TransportOsdViewModel viewModel, BaseItemDto media)
        {
            ImgLogo.Visibility = Visibility.Hidden;

            if (media != null && (media.HasLogo || !string.IsNullOrEmpty(media.ParentLogoItemId)))
            {
                try
                {
                    ImgLogo.Source =
                        await
                            viewModel.ImageManager.GetRemoteBitmapAsync(viewModel.ApiClient.GetLogoImageUrl(media,
                                new ImageOptions
                                {
                                }));

                    ImgLogo.Visibility = Visibility.Visible;
                }
                catch
                {
                    // Already logged at lower levels
                }
            }
        }

        /// <summary>
        /// Handles the DragStarted event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragStartedEventArgs" /> instance containing the event data.</param>
        private void CurrentPositionSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isPositionSliderUpdating = true;
        }

        /// <summary>
        /// Handles the DragCompleted event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragCompletedEventArgs" /> instance containing the event data.</param>
        private void CurrentPositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ViewModel.Seek(Convert.ToInt64(CurrentPositionSlider.Value));
            _isPositionSliderUpdating = false;
        }

        // Handles the PreviewMouseDown event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void CurrentPositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isPositionSliderUpdating = true;
            try
            {
                ViewModel.Seek(Convert.ToInt64(CurrentPositionSlider.Value));
            }
            finally
            {
                _isPositionSliderUpdating = false;
            }
        }
    }
}
