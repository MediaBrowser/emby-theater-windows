using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
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
        private bool _isPositionSliderDragging;

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

        void FullscreenVideoTransportOsd_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentPositionSlider.PreviewMouseUp += CurrentPositionSlider_PreviewMouseUp;
        }

        void FullscreenVideoTransportOsd_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged += vm_PropertyChanged;

                UpdateLogo(vm, vm.NowPlayingItem);
            }
        }

        void FullscreenVideoTransportOsd_Unloaded(object sender, RoutedEventArgs e)
        {
            CurrentPositionSlider.PreviewMouseUp -= CurrentPositionSlider_PreviewMouseUp;

            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= vm_PropertyChanged;
            }
        }

        void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            if (string.Equals(e.PropertyName, "NowPlayingItem"))
            {
                UpdateLogo(vm, vm.NowPlayingItem);
            }
            else if (string.Equals(e.PropertyName, "PositionTicks"))
            {
                if (!_isPositionSliderDragging)
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
                    ImgLogo.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(viewModel.ApiClient.GetLogoImageUrl(media, new ImageOptions
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
            _isPositionSliderDragging = true;
        }

        /// <summary>
        /// Handles the DragCompleted event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragCompletedEventArgs" /> instance containing the event data.</param>
        private void CurrentPositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isPositionSliderDragging = false;
        }

        /// <summary>
        /// Handles the PreviewMouseUp event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        void CurrentPositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Seek(Convert.ToInt64(CurrentPositionSlider.Value));
        }
    }
}
