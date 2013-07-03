using MediaBrowser.Model.Entities;
using System.Collections.Generic;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Interaction logic for ItemMediaInfo.xaml
    /// </summary>
    public partial class ItemMediaInfo : BaseDetailsControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemMediaInfo" /> class.
        /// </summary>
        public ItemMediaInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected override void OnItemChanged()
        {
            MediaStreams.Children.Clear();

            TxtPath.Text = Item.Path;

            if (Item.Video3DFormat.HasValue)
            {
                TxtVideoFormat.Visibility = Visibility.Visible;

                TxtVideoFormat.Text = "3D";
            }
            else
            {
                TxtVideoFormat.Visibility = Visibility.Collapsed;
            }

            foreach (var stream in Item.MediaStreams ?? new List<MediaStream> {})
            {
                MediaStreams.Children.Add(new MediaStreamControl
                {
                    MediaStream = stream
                });
            }
        }
    }
}
