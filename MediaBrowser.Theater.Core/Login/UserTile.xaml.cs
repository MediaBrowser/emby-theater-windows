using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Core.Login
{
    /// <summary>
    /// Interaction logic for UserTile.xaml
    /// </summary>
    public partial class UserTile : UserControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public UserDtoViewModel ViewModel
        {
            get { return DataContext as UserDtoViewModel; }
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        private UserDto Item
        {
            get { return ViewModel.User; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTile"/> class.
        /// </summary>
        public UserTile()
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
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReloadImage(Item);
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            var item = Item;

            ReloadImage(item);

            UserName.Text = item.Name;

            //if (item.LastActivityDate.HasValue)
            //{
            //    var date = item.LastActivityDate.Value.ToLocalTime();

            //    LastSeen.Text = "Last seen " + GetRelativeTimeText(date);
            //}
            //else
            //{
            //    LastSeen.Text = string.Empty;
            //}
        }

        /// <summary>
        /// Reloads the image.
        /// </summary>
        /// <param name="item">The item.</param>
        private async void ReloadImage(UserDto item)
        {
            await SetImageSource(item);
        }

        /// <summary>
        /// The true task result
        /// </summary>
        private static readonly Task TrueTaskResult = Task.FromResult(true);

        /// <summary>
        /// Sets the image source.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Task.</returns>
        private Task SetImageSource(UserDto item)
        {
            if (item.HasPrimaryImage)
            {
                var url = ViewModel.ApiClient.GetUserImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Primary
                });

                return SetImage(url);
            }

            SetDefaultImage();

            return TrueTaskResult;
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task.</returns>
        private async Task SetImage(string url)
        {
            try
            {
                GridDefaultImage.Visibility = Visibility.Collapsed;

                UserImage.Source = await ViewModel.ImageManager.GetRemoteBitmapAsync(url);

                UserImage.Visibility = Visibility.Visible;
            }
            catch (HttpException)
            {
                SetDefaultImage();
            }
        }

        /// <summary>
        /// Sets the default image.
        /// </summary>
        private void SetDefaultImage()
        {
            UserImage.Visibility = Visibility.Collapsed;
            GridDefaultImage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Gets the relative time text.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>System.String.</returns>
        private static string GetRelativeTimeText(DateTime date)
        {
            var ts = DateTime.Now - date;

            const int second = 1;
            const int minute = 60 * second;
            const int hour = 60 * minute;
            const int day = 24 * hour;
            const int month = 30 * day;

            var delta = Convert.ToInt32(ts.TotalSeconds);

            if (delta < 0)
            {
                return "not yet";
            }
            if (delta < 1 * minute)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 2 * minute)
            {
                return "a minute ago";
            }
            if (delta < 45 * minute)
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 90 * minute)
            {
                return "an hour ago";
            }
            if (delta < 24 * hour)
            {
                return ts.Hours == 1 ? "an hour ago" : ts.Hours + " hours ago";
            }
            if (delta < 48 * hour)
            {
                return "yesterday";
            }
            if (delta < 30 * day)
            {
                return ts.Days + " days ago";
            }
            if (delta < 12 * month)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }
}
