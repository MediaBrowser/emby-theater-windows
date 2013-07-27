using System.Linq;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for Reviews.xaml
    /// </summary>
    public partial class Reviews : UserControl
    {
        private readonly BaseItemDto _item;
        private readonly IApiClient _apiClient;
        private readonly IPresentationManager _presentation;

        public Reviews(BaseItemDto item, IApiClient apiClient, IPresentationManager presentation)
        {
            _item = item;
            _apiClient = apiClient;
            _presentation = presentation;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            OnItemChanged();
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected async void OnItemChanged()
        {
            try
            {
                var result = await _apiClient.GetCriticReviews(_item.Id);

                LoadReviews(result);
            }
            catch
            {
                _presentation.ShowDefaultErrorMessage();
            }
        }

        private void LoadReviews(ItemReviewsResult result)
        {
            LstItems.ItemsSource = CollectionViewSource.GetDefaultView(result.ItemReviews.Where(i => !string.IsNullOrEmpty(i.Caption)));
        }
    }
}
