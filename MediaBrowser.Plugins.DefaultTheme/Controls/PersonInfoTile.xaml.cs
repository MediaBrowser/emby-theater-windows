using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for PersonInfoTile.xaml
    /// </summary>
    public partial class PersonInfoTile : UserControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public BaseItemDtoViewModel ViewModel
        {
            get { return DataContext as BaseItemDtoViewModel; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemTile" /> class.
        /// </summary>
        public PersonInfoTile()
        {
            InitializeComponent();

            DataContextChanged += BaseItemTile_DataContextChanged;
            Loaded += BaseItemTile_Loaded;
            Unloaded += BaseItemTile_Unloaded;
        }

        /// <summary>
        /// Handles the Unloaded event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the DataContextChanged event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnItemChanged();

            var vm = ViewModel;

            if (vm != null)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReloadImage(ViewModel);
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            var viewModel = ViewModel;

            var item = viewModel.Item;

            ReloadImage(viewModel);

            TxtName.Text = item.Name;

            if (!string.IsNullOrEmpty(viewModel.PersonRole))
            {
                TxtRole.Text = " as " + viewModel.PersonRole;
            }
            else if (!string.IsNullOrEmpty(viewModel.PersonType))
            {
                TxtRole.Text = viewModel.PersonType;
            }
            else
            {
                TxtRole.Text = string.Empty;
            }
        }

        /// <summary>
        /// Reloads the image.
        /// </summary>
        private async void ReloadImage(BaseItemDtoViewModel viewModel)
        {
            if (viewModel.ImageDisplayWidth.Equals(0) || viewModel.ImageDisplayWidth.Equals(0))
            {
                return;
            }

            await SetImageSource(viewModel);
        }

        private static readonly Task TrueTaskResult = Task.FromResult(true);

        private Task SetImageSource(BaseItemDtoViewModel viewModel)
        {
            var item = ViewModel.Item;

            if (item.HasPrimaryImage)
            {
                var url = viewModel.GetImageUrl(ImageType.Primary);

                return SetImage(viewModel, url);
            }

            SetDefaultImage(viewModel);

            return TrueTaskResult;
        }

        private async Task SetImage(BaseItemDtoViewModel viewModel, string url)
        {
            try
            {
                ItemImage.Source = await viewModel.ImageManager.GetRemoteBitmapAsync(url);

                GridDefaultImage.Visibility = Visibility.Collapsed;
                ItemImage.Visibility = Visibility.Visible;
            }
            catch (HttpException)
            {
                SetDefaultImage(viewModel);
            }
        }

        /// <summary>
        /// Sets the default image.
        /// </summary>
        private void SetDefaultImage(BaseItemDtoViewModel viewModel)
        {
            GridDefaultImage.Visibility = Visibility.Visible;
            ItemImage.Visibility = Visibility.Collapsed;
        }
    }
}
