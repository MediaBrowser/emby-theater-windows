using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.Playlist
{
    public class Playlist : PlayerExtension<PlaylistSettings, PlaylistConfigDialog>
    {
        private const string Subcategory = "Playlist";

        private readonly PlaylistForm form = new PlaylistForm();
        
        private Form mpdnForm;
        private Point mpdnStartLocation;
        private Point formStartLocation;
        private bool moving;

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("A1997E34-D67B-43BB-8FE6-55A71AE7184B"),
                    Name = "Playlist",
                    Description = "Adds playlist support with advanced capabilities",
                    Copyright = "Enhanced by Garteal"
                };
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            form.Setup(this);

            PlayerControl.PlayerStateChanged += OnPlayerStateChange;
            PlayerControl.FormClosed += OnFormClosed;
            PlayerControl.DragEnter += OnDragEnter;
            PlayerControl.DragDrop += OnDragDrop;
            PlayerControl.CommandLineFileOpen += OnCommandLineFileOpen;
            mpdnForm = PlayerControl.Form;
            mpdnForm.Move += OnMpdnFormMove;
            form.Move += OnFormMove;

            if (Settings.RememberWindowBounds)
            {
                form.RememberWindowBounds = Settings.RememberWindowBounds;
                form.WindowBounds = Settings.WindowBounds;
            }

            if (Settings.ShowPlaylistOnStartup)
            {
                form.Show(PlayerControl.VideoPanel);
            }

            if (Settings.Autoplay)
            {
                form.Autoplay = Settings.Autoplay;
            }

            if (Settings.RememberLastPlayedFile)
            {
                string[] files = { Settings.LastPlayedFile };
                form.AddFiles(files);
            }
        }

        public override void Destroy()
        {
            PlayerControl.PlayerStateChanged -= OnPlayerStateChange;
            PlayerControl.FormClosed -= OnFormClosed;
            PlayerControl.DragEnter -= OnDragEnter;
            PlayerControl.DragDrop -= OnDragDrop;
            PlayerControl.CommandLineFileOpen -= OnCommandLineFileOpen;
            mpdnForm.Move -= OnMpdnFormMove;
            form.Move -= OnFormMove;

            base.Destroy();
            form.Dispose();
        }

        private void OnPlayerStateChange(object sender, EventArgs e)
        {
            if (!Settings.AddFileToPlaylistOnOpen) return;
            if (PlayerControl.MediaFilePath == "") return;
            var foundFile = form.Playlist.Find(i => i.FilePath == PlayerControl.MediaFilePath);
            if (foundFile != null) return;
            form.AddActiveFile(PlayerControl.MediaFilePath);
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            Settings.WindowBounds = form.Bounds;

            if (form.CurrentItem != null && form.CurrentItem.FilePath != "")
            {
                Settings.LastPlayedFile = form.CurrentItem.FilePath;
            }
        }

        private void OnFormMove(object sender, EventArgs e)
        {
            if (moving)
                return;

            mpdnStartLocation = PlayerControl.Form.Location;
            formStartLocation = form.Location;
        }

        private void OnMpdnFormMove(object sender, EventArgs e)
        {
            moving = true;
            form.Left = formStartLocation.X + PlayerControl.Form.Location.X - mpdnStartLocation.X;
            form.Top = formStartLocation.Y + PlayerControl.Form.Location.Y - mpdnStartLocation.Y;
            moving = false;
        }

        public override IList<Verb> Verbs
        {
            get
            {
                return new[]
                {
                    new Verb(Category.File, string.Empty, "Open Playlist", "Ctrl+Alt+O", string.Empty, OpenPlaylist),
                    new Verb(Category.View, string.Empty, "Playlist", "Ctrl+Alt+P", string.Empty, ViewPlaylist),
                    new Verb(Category.Play, Subcategory, "Next", "Ctrl+Alt+N", string.Empty, () => form.PlayNext()),
                    new Verb(Category.Play, Subcategory, "Previous", "Ctrl+Alt+B", string.Empty, () => form.PlayPrevious())
                };
            }
        }

        public static bool IsPlaylistFile(string filename)
        {
            var extension = Path.GetExtension(filename);
            return extension != null && extension.ToLower() == ".mpl";
        }

        private void OpenPlaylist()
        {
            form.Show(PlayerControl.VideoPanel);
            form.OpenPlaylist();
        }

        private void ViewPlaylist()
        {
            form.Show(PlayerControl.VideoPanel);
        }

        private void OnDragEnter(object sender, PlayerControlEventArgs<DragEventArgs> e)
        {
            e.Handled = true;
            e.InputArgs.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, PlayerControlEventArgs<DragEventArgs> e)
        {
            var files = (string[])e.InputArgs.Data.GetData(DataFormats.FileDrop);

            if (Settings.AddFileToPlaylistOnOpen)
            {
                e.Handled = true;

                if (files.Length == 1)
                {
                    var filename = files[0];
                    if (Playlist.IsPlaylistFile(filename))
                    {
                        form.OpenPlaylist(filename);
                        return;
                    }
                }

                form.AddFiles(files);
            }
            else
            {
                if (files.Length > 1)
                {
                    e.Handled = true;
                    // Add multiple files to playlist
                    form.Show(PlayerControl.VideoPanel);
                    form.AddFiles(files);
                }
                else
                {
                    var filename = files[0];
                    if (IsPlaylistFile(filename))
                    {
                        // Playlist file
                        form.OpenPlaylist(filename);
                        form.Show(PlayerControl.VideoPanel);
                        e.Handled = true;
                    }
                }
            }
        }

        private void OnCommandLineFileOpen(object sender, CommandLineFileOpenEventArgs e)
        {
            if (!IsPlaylistFile(e.Filename)) return;
            e.Handled = true;
            form.OpenPlaylist(e.Filename);
        }

        #region RemoteMethods

        public PlaylistForm GetPlaylistForm
        {
            get { return form; }
        }

        #endregion
    }

    public class PlaylistSettings
    {
        public bool ShowPlaylistOnStartup { get; set; }
        public bool Autoplay { get; set; }
        public bool AddFileToPlaylistOnOpen { get; set; }
        public bool RememberWindowBounds { get; set; }
        public bool RememberLastPlayedFile { get; set; }
        public Rectangle WindowBounds { get; set; }
        public string LastPlayedFile { get; set; }

        public PlaylistSettings()
        {
            ShowPlaylistOnStartup = false;
            Autoplay = false;
            AddFileToPlaylistOnOpen = false;
            RememberWindowBounds = false;
            RememberLastPlayedFile = false;
        }
    }
}
