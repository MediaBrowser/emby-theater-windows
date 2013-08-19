using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.ListPage
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        /// <summary>
        /// The _item
        /// </summary>
        private ItemViewModel _viewModel;

        private ItemViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;
                OnItemChanged();
            }
        }

        public Sidebar()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContextChanged += ItemInfoFooter_DataContextChanged;

            ViewModel = DataContext as ItemViewModel;
        }

        void ItemInfoFooter_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as ItemViewModel;
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected void OnItemChanged()
        {
            if (ViewModel != null)
            {
                OnItemChanged(ViewModel, ViewModel.Item);
            }
        }

        private async void OnItemChanged(ItemViewModel viewModel, BaseItemDto item)
        {
            UpdateLogo(viewModel, item);

            UpdateArt(viewModel, item);

            TxtGenres.Visibility = item.Genres.Count > 0 && !item.IsType("episode") && !item.IsType("season") ? Visibility.Visible : Visibility.Collapsed;

            TxtGenres.Text = string.Join(" / ", item.Genres.Take(3).ToArray());
        }

        private CancellationTokenSource _logoCancellationTokenSource;
        
        private async void UpdateLogo(ItemViewModel viewModel, BaseItemDto item)
        {
            if (_logoCancellationTokenSource != null)
            {
                _logoCancellationTokenSource.Cancel();
                _logoCancellationTokenSource.Dispose();
            }

            _logoCancellationTokenSource = new CancellationTokenSource();

            if (item != null && (item.HasLogo || item.ParentLogoImageTag.HasValue))
            {
                try
                {
                    var token = _logoCancellationTokenSource.Token;

                    var img = await viewModel.GetBitmapImageAsync(new ImageOptions
                    {
                        ImageType = ImageType.Logo

                    }, token);

                    token.ThrowIfCancellationRequested();

                    LogoImage.Source = img;

                    LogoImage.Visibility = Visibility.Visible;
                    TxtTitle.Visibility = Visibility.Collapsed;
                }
                catch (OperationCanceledException)
                {
                }
                catch
                {
                    ArtImage.Visibility = Visibility.Collapsed;
                    TxtTitle.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ArtImage.Visibility = Visibility.Collapsed;
                TxtTitle.Visibility = Visibility.Visible;
            }
        }

        private CancellationTokenSource _artCancellationTokenSource;

        private async void UpdateArt(ItemViewModel viewModel, BaseItemDto item)
        {
            const int maxheight = 120;

            if (_artCancellationTokenSource != null)
            {
                _artCancellationTokenSource.Cancel();
                _artCancellationTokenSource.Dispose();
            }

            _artCancellationTokenSource = new CancellationTokenSource();

            if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
            {
                try
                {
                    var token = _artCancellationTokenSource.Token;
                    
                    var img = await viewModel.GetBitmapImageAsync(new ImageOptions
                    {
                        Height = maxheight,
                        ImageType = ImageType.Art

                    }, token);

                    token.ThrowIfCancellationRequested();

                    ArtImage.Source = img;

                    ArtImage.Visibility = Visibility.Visible;
                }
                catch (OperationCanceledException)
                {
                }
                catch
                {
                    ArtImage.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ArtImage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
