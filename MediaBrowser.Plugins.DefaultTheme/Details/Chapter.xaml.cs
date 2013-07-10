using System.Net;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for Chapter.xaml
    /// </summary>
    public partial class Chapter : UserControl
    {
        public Chapter()
        {
            InitializeComponent();

            DataContextChanged += BaseItemTile_DataContextChanged;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public ChapterInfoDtoViewModel ViewModel
        {
            get { return DataContext as ChapterInfoDtoViewModel; }
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        private ChapterInfoDto ChapterInfoDto
        {
            get { return ViewModel.Chapter; }
        }

        /// <summary>
        /// Handles the DataContextChanged event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnItemChanged();
        }

        private async void ReloadImage()
        {
            var width = 288;

            var height = ChapterInfoDtoViewModel.GetChapterImageHeight(ViewModel.Item, width, 162);

            ChapterImage.Width = width;
            ChapterImage.Height = height;
            ImageBorder.Width = width;
            ImageBorder.Height = height;

            if (ChapterInfoDto.HasImage)
            {
                try
                {
                    ChapterImage.Source = await ViewModel.GetImage(new ImageOptions
                    {
                        Width = width,
                        Height = Convert.ToInt32(height)
                    });

                    ChapterImage.Visibility = Visibility.Visible;
                    ImageBorder.Visibility = Visibility.Collapsed;
                    return;
                }
                catch (HttpException)
                {

                }
            }

            ChapterImage.Visibility = Visibility.Collapsed;
            ImageBorder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            ReloadImage();
            TxtTime.Text = GetMinutesString(ChapterInfoDto);
        }

        private string GetMinutesString(ChapterInfoDto item)
        {
            var time = TimeSpan.FromTicks(item.StartPositionTicks);

            return time.ToString(time.TotalHours < 1 ? "m':'ss" : "h':'mm':'ss");
        }
    }
}
