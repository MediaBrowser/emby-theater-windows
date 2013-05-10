using System.Collections.Generic;
using MediaBrowser.Model.Dto;
using System;
using System.Linq;
using System.Windows;
using MediaBrowser.UI.Controller;
using MediaBrowser.UI.Playback;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Interaction logic for ItemOverview.xaml
    /// </summary>
    public partial class ItemOverview : BaseDetailsControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemOverview" /> class.
        /// </summary>
        public ItemOverview()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected override void OnItemChanged()
        {
            var directors = (Item.People ?? new BaseItemPerson[] { }).Where(p => string.Equals(p.Type, "director", StringComparison.OrdinalIgnoreCase)).ToList();

            if (directors.Count > 0)
            {
                PnlDirectors.Visibility = Visibility.Visible;

                Directors.Text = string.Join(" / ", directors.Take(3).Select(d => d.Name).ToArray());
                DirectorLabel.Text = directors.Count > 1 ? "directors" : "director";
            }
            else
            {
                PnlDirectors.Visibility = Visibility.Collapsed;
            }

            if (Item.Genres != null && Item.Genres.Count > 0)
            {
                PnlGenres.Visibility = Visibility.Visible;

                Genres.Text = string.Join(" / ", Item.Genres.Take(4).ToArray());
                GenreLabel.Text = Item.Genres.Count > 1 ? "genres" : "genre";
            }
            else
            {
                PnlGenres.Visibility = Visibility.Collapsed;
            }

            if (Item.Studios != null && Item.Studios.Length > 0)
            {
                PnlStudios.Visibility = Visibility.Visible;

                Studios.Text = string.Join(" / ", Item.Studios.Select(i => i.Name).Take(3).ToArray());
                StudiosLabel.Text = Item.Studios.Length > 1 ? "studios" : "studio";
            }
            else
            {
                PnlStudios.Visibility = Visibility.Collapsed;
            }

            if (Item.PremiereDate.HasValue)
            {
                PnlPremiereDate.Visibility = Visibility.Visible;

                PremiereDate.Text = Item.PremiereDate.Value.ToShortDateString();
            }
            else
            {
                PnlPremiereDate.Visibility = Visibility.Collapsed;
            }

            var artists = Item.Artists == null ? string.Empty : string.Join(",", Item.Artists);

            if (!string.IsNullOrEmpty(artists))
            {
                PnlArtist.Visibility = Visibility.Visible;
                Artist.Text = artists;
            }
            else
            {
                PnlArtist.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(Item.Album))
            {
                PnlAlbum.Visibility = Visibility.Visible;
                Album.Text = artists;
            }
            else
            {
                PnlAlbum.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(Item.AlbumArtist))
            {
                PnlAlbumArtist.Visibility = Visibility.Visible;
                AlbumArtist.Text = artists;
            }
            else
            {
                PnlAlbumArtist.Visibility = Visibility.Collapsed;
            }
    
            Overview.Text = Item.Overview;
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            UIKernel.Instance.PlaybackManager.Play(new PlayOptions
            {
                Items = new List<BaseItemDto> { Item }
            });
        }
    }
}
