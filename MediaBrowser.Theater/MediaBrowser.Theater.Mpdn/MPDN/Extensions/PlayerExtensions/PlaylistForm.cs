// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mpdn.PlayerExtensions.Playlist
{
    public partial class PlaylistForm : FormEx
    {
        private Playlist playListUi;

        public static int PlaylistCount { get; set; }

        private const double MaxOpacity = 1.0;
        private const double MinOpacity = 0.7;
        private const string ActiveIndicator = "[*]";
        private const string InactiveIndicator = "[ ]";

        private bool firstShow = true;
        private bool wasShowing;

        public List<PlaylistItem> Playlist { get; set; }
        public PlaylistItem CurrentItem { get; set; }

        private int currentPlayIndex = -1;
        private long previousChapterPosition;

        private Rectangle dragRowRect;
        private int dragRowIndex;

        public Rectangle WindowBounds { get; set; }
        public bool RememberWindowBounds { get; set; }
        public bool Autoplay { get; set; }

        #region Events

        public event EventHandler PlaylistChanged;

        #endregion

        public PlaylistForm()
        {
            InitializeComponent();
            Opacity = MinOpacity;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                if (Playlist != null)
                {
                    PlayerControl.PlayerStateChanged -= PlayerStateChanged;
                    PlayerControl.PlaybackCompleted -= PlaybackCompleted;
                    PlayerControl.FrameDecoded -= FrameDecoded;
                    PlayerControl.FramePresented -= FramePresented;
                    PlayerControl.EnteringFullScreenMode -= EnteringFullScreenMode;
                    PlayerControl.ExitedFullScreenMode -= ExitedFullScreenMode;
                }
            }
            base.Dispose(disposing);
        }

        public void Show(Control owner)
        {
            if (PlayerControl.InFullScreenMode)
                return;

            Hide();
            SetLocation(owner);
            timer.Enabled = true;
            dgv_PlayList.Focus();
            base.Show(owner);
        }

        public void Setup(Playlist playListUi)
        {
            if (Playlist != null)
                return;

            this.playListUi = playListUi;
            Icon = PlayerControl.ApplicationIcon;

            dgv_PlayList.CellFormatting += dgv_PlayList_CellFormatting;
            dgv_PlayList.CellPainting += dgv_PlayList_CellPainting;
            dgv_PlayList.CellDoubleClick += dgv_PlayList_CellDoubleClick;
            dgv_PlayList.CellEndEdit += dgv_PlayList_CellEndEdit;
            dgv_PlayList.EditingControlShowing += dgv_PlayList_EditingControlShowing;
            dgv_PlayList.MouseMove += dgv_PlayList_MouseMove;
            dgv_PlayList.MouseDown += dgv_PlayList_MouseDown;
            dgv_PlayList.DragOver += dgv_PlayList_DragOver;
            dgv_PlayList.DragDrop += dgv_PlayList_DragDrop;

            PlayerControl.PlayerStateChanged += PlayerStateChanged;
            PlayerControl.PlaybackCompleted += PlaybackCompleted;
            PlayerControl.FrameDecoded += FrameDecoded;
            PlayerControl.FramePresented += FramePresented;
            PlayerControl.EnteringFullScreenMode += EnteringFullScreenMode;
            PlayerControl.ExitedFullScreenMode += ExitedFullScreenMode;

            Playlist = new List<PlaylistItem>();
        }

        public void ClearPlaylist()
        {
            Playlist.Clear();
            currentPlayIndex = -1;
        }

        public void PopulatePlaylist()
        {
            dgv_PlayList.Rows.Clear();
            if (Playlist.Count == 0) return;

            foreach (var i in Playlist)
            {
                if (i.SkipChapters != null)
                {
                    if (i.EndChapter != -1)
                    {
                        dgv_PlayList.Rows.Add(new Bitmap(25, 25), i.FilePath, String.Join(",", i.SkipChapters), i.EndChapter);
                    }
                    else
                    {
                        dgv_PlayList.Rows.Add(new Bitmap(25, 25), i.FilePath, String.Join(",", i.SkipChapters));
                    }
                }
                else
                {
                    dgv_PlayList.Rows.Add(new Bitmap(25, 25), i.FilePath);
                }
            }

            if (PlayerControl.MediaFilePath != "" && Playlist.Count > 0)
            {
                currentPlayIndex = (Playlist.FindIndex(i => i.Active) > -1) ? Playlist.FindIndex(i => i.Active) : 0;
            }
            NotifyPlaylistChanged();
            PlaylistCount = Playlist.Count;
        }

        public void NewPlaylist()
        {
            ClearPlaylist();
            PopulatePlaylist();
            CloseMedia();
        }

        public void OpenPlaylist()
        {
            openPlaylistDialog.FileName = savePlaylistDialog.FileName;
            if (openPlaylistDialog.ShowDialog(PlayerControl.Form) != DialogResult.OK) return;

            OpenPlaylist(openPlaylistDialog.FileName);
        }

        public void OpenPlaylist(string fileName)
        {
            ClearPlaylist();

            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains("|"))
                        {
                            ParseWithChapters(line);
                        }
                        else
                        {
                            ParseWithoutChapters(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid or corrupt playlist file.\nAdditional info: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            PopulatePlaylist();
            PlayActive();
        }

        public void SavePlaylist()
        {
            if (Playlist.Count == 0) return;

            savePlaylistDialog.FileName = openPlaylistDialog.FileName;
            if (savePlaylistDialog.ShowDialog(PlayerControl.Form) != DialogResult.OK) return;

            SavePlaylist(savePlaylistDialog.FileName);
        }

        public void SavePlaylist(string filename)
        {
            IEnumerable<string> playlist;
            bool containsChapter = false;

            foreach (var item in Playlist)
            {
                if (item.HasChapter)
                {
                    containsChapter = true;
                }
            }

            if (containsChapter)
            {
                playlist =
                    Playlist
                        .Select(
                            item =>
                                string.Format("{0}{1} | SkipChapter: {2} | EndChapter: {3}",
                                    item.Active ? ActiveIndicator : InactiveIndicator,
                                    item.FilePath, item.HasChapter ? String.Join(",", item.SkipChapters) : "0",
                                    item.EndChapter > -1 ? item.EndChapter : 0));
            }
            else
            {
                playlist =
                    Playlist
                        .Select(
                            item =>
                                string.Format("{0}{1}", item.Active ? ActiveIndicator : InactiveIndicator,
                                    item.FilePath));
            }

            File.WriteAllLines(filename, playlist, Encoding.UTF8);
        }

        public void PlayActive()
        {
            currentPlayIndex = -1;

            foreach (var item in Playlist)
            {
                currentPlayIndex++;
                if (!item.Active) continue;
                OpenMedia();
                return;
            }

            currentPlayIndex = 0;
            OpenMedia();
        }

        public void PlayNext()
        {
            currentPlayIndex++;
            OpenMedia();

            if (currentPlayIndex < Playlist.Count) return;
            currentPlayIndex = Playlist.Count - 1;
        }

        public void PlayPrevious()
        {
            currentPlayIndex--;
            OpenMedia();

            if (currentPlayIndex >= 0) return;
            currentPlayIndex = 0;
        }

        public void AddActiveFile(string fileName)
        {
            var foundFile = Playlist.Find(i => i.FilePath == fileName);
            if (foundFile != null) return;

            ResetActive();
            var item = new PlaylistItem(fileName, true);
            Playlist.Add(item);
            CurrentItem = item;
            PopulatePlaylist();
            Text = PlayerControl.PlayerState + " ─ " + CurrentItem.FilePath;
        }

        public void AddFiles(string[] fileNames)
        {
            var files = fileNames.Except(Playlist.Select(item => item.FilePath)).ToArray();

            foreach (var item in files.Select(s => new PlaylistItem(s, false) { EndChapter = -1 }))
            {
                Playlist.Add(item);
            }

            PopulatePlaylist();

            if (!Autoplay) return;

            currentPlayIndex = Playlist.Count > 0 ? Playlist.Count - 1 : 0;
            OpenMedia();
        }

        public void InsertFile(int index, string fileName)
        {
            PlaylistItem item = new PlaylistItem(fileName, false);
            Playlist.Insert(index, item);
            PopulatePlaylist();
        }

        public void RemoveFile(int index)
        {
            Playlist.RemoveAt(index);
            PopulatePlaylist();
        }

        public void CloseMedia()
        {
            try
            {
                currentPlayIndex = -1;
                CurrentItem = null;
                Text = "Playlist";
                PlayerControl.CloseMedia();
            }
            catch (Exception ex)
            {
                PlayerControl.HandleException(ex);
            }
        }

        public void SetPlaylistIndex(int index)
        {
            currentPlayIndex = index;
            OpenMedia();
        }

        private void SetLocation(Control owner)
        {
            if (RememberWindowBounds)
            {
                if (!firstShow) return;
                var rect = WindowBounds;
                Left = rect.Left;
                Top = rect.Top;
                Width = rect.Width;
                Height = rect.Height;
                firstShow = false;
            }
            else
            {
                if (!firstShow) return;
                var screen = Screen.FromControl(owner);
                var screenBounds = screen.WorkingArea;
                var p = owner.PointToScreen(new Point(owner.Right, owner.Bottom));
                var left = p.X - Width/(int) (5*ScaleFactor.Width);
                var top = p.Y - Height/(int) (5*ScaleFactor.Height);
                Left = left + Width > screenBounds.Right ? screenBounds.Right - Width : left;
                Top = top + Height > screenBounds.Bottom ? screenBounds.Bottom - Height : top;
                firstShow = false;
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            HandleOpacity();
        }

        private void HandleOpacity()
        {
            var pos = MousePosition;
            bool inForm = pos.X >= Left && pos.Y >= Top && pos.X < Right && pos.Y < Bottom;

            if (inForm)
            {
                if (Opacity < MaxOpacity)
                {
                    Opacity += 0.1;
                }
            }
            else if (Opacity > MinOpacity)
            {
                Opacity -= 0.1;
            }
        }

        void dgv_PlayList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var skipChapterCell = dgv_PlayList.Rows[e.RowIndex].Cells[2];
            var endChapterCell = dgv_PlayList.Rows[e.RowIndex].Cells[3];

            if (skipChapterCell.IsInEditMode || endChapterCell.IsInEditMode)
            {
                e.CellStyle.ForeColor = Color.Black;
            }
        }

        void dgv_PlayList_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.ClipBounds, DataGridViewPaintParts.All);

            bool paintPlayRow = CurrentItem != null && e.RowIndex == currentPlayIndex;
            if (!paintPlayRow) return;

            var brush = new SolidBrush(Color.FromArgb(42, 127, 183));

            if (e.ColumnIndex == 0)
            {
                var rect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 4, e.CellBounds.Width, e.CellBounds.Height - 9);
                var playIcon = (Bitmap) PlayButton.BackgroundImage;
                var offset = new Point(e.CellBounds.X, e.CellBounds.Y + 2);
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawImage(playIcon, offset);
            }
            else
            {
                var rect = new Rectangle(e.CellBounds.X, e.CellBounds.Y + 4, e.CellBounds.Width, e.CellBounds.Height - 9);
                e.Graphics.FillRectangle(brush, rect);
            }

            e.Paint(e.ClipBounds, DataGridViewPaintParts.ContentForeground);
            e.Handled = true;
        }

        private void dgv_PlayList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv_PlayList.Rows.Count <= 0) return;
            if (e.ColumnIndex > 1) return;
            currentPlayIndex = e.RowIndex;
            OpenMedia();
        }

        private void dgv_PlayList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ParseChapterInput();
        }

        void dgv_PlayList_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= dgv_PlayList_HandleInput;
            if (dgv_PlayList.CurrentCell.ColumnIndex <= 1) return;

            var tb = e.Control as TextBox;
            if (tb != null)
            {
                tb.KeyPress += dgv_PlayList_HandleInput;
            }
        }

        private void dgv_PlayList_HandleInput(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != ' ' && dgv_PlayList.CurrentCell.ColumnIndex == 2)
            {
                var toolTip = new ToolTip();
                var cell = dgv_PlayList.CurrentCell;
                var cellDisplayRect = dgv_PlayList.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, false);
                toolTip.Show("Only numbers are allowed. You may separate them with a comma or a space.", dgv_PlayList,
                              cellDisplayRect.X + cell.Size.Width / 2,
                              cellDisplayRect.Y + cell.Size.Height / 2,
                              2000);

                e.Handled = true;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && dgv_PlayList.CurrentCell.ColumnIndex == 3)
            {
                var toolTip = new ToolTip();
                var cell = dgv_PlayList.CurrentCell;
                var cellDisplayRect = dgv_PlayList.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, false);
                toolTip.Show("Only numbers are allowed.", dgv_PlayList,
                              cellDisplayRect.X + cell.Size.Width / 2,
                              cellDisplayRect.Y + cell.Size.Height / 2,
                              2000);

                e.Handled = true;
            }
        }

        void dgv_PlayList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (dragRowRect != Rectangle.Empty && !dragRowRect.Contains(e.X, e.Y))
            {
                dgv_PlayList.DoDragDrop(dgv_PlayList.Rows[dragRowIndex], DragDropEffects.Move);
            }
        }

        void dgv_PlayList_MouseDown(object sender, MouseEventArgs e)
        {
            dragRowIndex = dgv_PlayList.HitTest(e.X, e.Y).RowIndex;

            if (dragRowIndex != -1)
            {
                var dragSize = SystemInformation.DragSize;
                dragRowRect = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            {
                dragRowRect = Rectangle.Empty;
            }
        }

        void dgv_PlayList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        void dgv_PlayList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    var filename = files[0];
                    if (PlayerExtensions.Playlist.Playlist.IsPlaylistFile(filename))
                    {
                        OpenPlaylist(filename);
                        return;
                    }
                }

                AddFiles(files);
                dgv_PlayList.CurrentCell = dgv_PlayList.Rows[dgv_PlayList.Rows.Count - 1].Cells[1];
            }
            else if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                var clientPoint = dgv_PlayList.PointToClient(new Point(e.X, e.Y));
                int destinationRow = dgv_PlayList.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

                var playItem = Playlist.ElementAt(dragRowIndex);
                Playlist.RemoveAt(dragRowIndex);
                NotifyPlaylistChanged();
                Playlist.Insert(destinationRow, playItem);
                PopulatePlaylist();
                dgv_PlayList.CurrentCell = dgv_PlayList.Rows[destinationRow].Cells[1];
            }
        }

        private void PlayerStateChanged(object sender, PlayerStateEventArgs e)
        {
            if (CurrentItem == null) return;
            Text = PlayerControl.PlayerState + " - " + CurrentItem.FilePath;
        }

        private void PlaybackCompleted(object sender, EventArgs e)
        {
            if (PlayerControl.PlayerState == PlayerState.Closed) return;
            if (PlayerControl.MediaPosition == PlayerControl.MediaDuration)
            {
                PlayNext();
            }
        }

        private void EnteringFullScreenMode(object sender, EventArgs e)
        {
            wasShowing = Visible;
            Hide();
        }

        private void ExitedFullScreenMode(object sender, EventArgs e)
        {
            if (wasShowing)
            {
                Show(PlayerControl.VideoPanel);
            }
        }

        private void PlaylistFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true;
            Hide();
            timer.Enabled = false;
        }

        private void FrameDecoded(object sender, FrameEventArgs e)
        {
            if (PlayerControl.MediaFilePath != "" && PlayerControl.Chapters.Count != 0 && CurrentItem != null && CurrentItem.HasChapter)
            {
                previousChapterPosition = GetChapters().Aggregate((prev, next) => e.SampleTime >= prev.Position && e.SampleTime <= next.Position ? prev : next).Position;
            }
        }

        private void FramePresented(object sender, FrameEventArgs e)
        {
            if (PlayerControl.MediaFilePath != "" && PlayerControl.Chapters.Count != 0 && CurrentItem != null && CurrentItem.HasChapter)
            {
                if (e.SampleTime >= previousChapterPosition)
                {
                    int currentChapterIndex = GetChapterIndexByPosition(previousChapterPosition);

                    if (CurrentItem.SkipChapters.Contains(currentChapterIndex) && currentChapterIndex < PlayerControl.Chapters.Count)
                    {
                        SelectChapter(currentChapterIndex);
                    }
                    else if (currentChapterIndex == CurrentItem.EndChapter)
                    {
                        PlayNext();
                    }
                }
            }
        }

        private void ParseChapterInput()
        {
            try
            {
                for (int i = 0; i < dgv_PlayList.Rows.Count; i++)
                {
                    var skipChapterCell = dgv_PlayList.Rows[i].Cells[2];
                    var endChapterCell = dgv_PlayList.Rows[i].Cells[3];

                    if (skipChapterCell.Value != null && skipChapterCell.Value.ToString() != "")
                    {
                        var formattedValue = Regex.Replace(skipChapterCell.Value.ToString(), @"[^0-9,\s]*", "");
                        var numbers = formattedValue.Trim().Replace(" ", ",").Split(',');
                        var sortedNumbers = numbers.Distinct().Except(new[] { "" }).Select(int.Parse).OrderBy(x => x).ToList();

                        if (CurrentItem != null && i == currentPlayIndex)
                        {
                            if (sortedNumbers.Any(num => num >= PlayerControl.Chapters.Count))
                            {
                                var toolTip = new ToolTip();
                                var cellDisplayRect = dgv_PlayList.GetCellDisplayRectangle(skipChapterCell.ColumnIndex,
                                    skipChapterCell.RowIndex, false);
                                toolTip.Show("Only numbers < " + PlayerControl.Chapters.Count + " are allowed",
                                    dgv_PlayList,
                                    cellDisplayRect.X + skipChapterCell.Size.Width / 2,
                                    cellDisplayRect.Y + skipChapterCell.Size.Height / 2,
                                    2000);
                                sortedNumbers.RemoveAll(num => num >= PlayerControl.Chapters.Count);
                            }
                            if (PlayerControl.Chapters.Count == 0)
                            {
                                sortedNumbers.Clear();
                            }
                        }

                        formattedValue = String.Join(",", sortedNumbers);
                        skipChapterCell.Value = formattedValue;
                    }

                    if (endChapterCell.Value != null && endChapterCell.Value.ToString() != "")
                    {
                        var value = new String(endChapterCell.Value.ToString().Where(Char.IsDigit).ToArray());

                        if (CurrentItem != null && i == currentPlayIndex)
                        {
                            if (value.Length > 0 && int.Parse(value) > PlayerControl.Chapters.Count)
                            {
                                var toolTip = new ToolTip();
                                var cellDisplayRect = dgv_PlayList.GetCellDisplayRectangle(endChapterCell.ColumnIndex,
                                    endChapterCell.RowIndex, false);
                                toolTip.Show("Only numbers <= " + PlayerControl.Chapters.Count + " are allowed",
                                    dgv_PlayList,
                                    cellDisplayRect.X + endChapterCell.Size.Width / 2,
                                    cellDisplayRect.Y + endChapterCell.Size.Height / 2,
                                    2000);

                                value = PlayerControl.Chapters.Count.ToString();
                            }
                            if (PlayerControl.Chapters.Count == 0)
                            {
                                value = "";
                            }
                        }

                        endChapterCell.Value = value;
                    }
                }

                UpdatePlaylist();
            }
            catch (Exception ex)
            {
                PlayerControl.HandleException(ex);
            }
        }

        private void UpdatePlaylist()
        {
            try
            {
                for (int i = 0; i < Playlist.Count; i++)
                {
                    var skipChapters = new List<int>();
                    int endChapter = -1;

                    var skipChapterCell = dgv_PlayList.Rows[i].Cells[2];
                    var endChapterCell = dgv_PlayList.Rows[i].Cells[3];

                    if (skipChapterCell.Value != null && skipChapterCell.Value.ToString() != "")
                    {
                        skipChapters = skipChapterCell.Value.ToString().Split(',').Select(int.Parse).ToList();
                        Playlist.ElementAt(i).HasChapter = true;
                    }

                    if (endChapterCell.Value != null && endChapterCell.Value.ToString() != "")
                    {
                        endChapter = int.Parse(endChapterCell.Value.ToString());
                    }

                    Playlist.ElementAt(i).SkipChapters = skipChapters;
                    Playlist.ElementAt(i).EndChapter = endChapter;
                }
            }
            catch (Exception ex)
            {
                PlayerControl.HandleException(ex);
            }
        }

        private void ParseWithoutChapters(string line)
        {
            string title = "";
            bool isActive = false;

            if (line.StartsWith(ActiveIndicator))
            {
                title = line.Substring(ActiveIndicator.Length).Trim();
                isActive = true;
            }
            else if (line.StartsWith(InactiveIndicator))
            {
                title = line.Substring(InactiveIndicator.Length).Trim();
            }
            else
            {
                throw new FileLoadException();
            }
            Playlist.Add(new PlaylistItem(title, isActive));
        }

        private void ParseWithChapters(string line)
        {
            var splitLine = line.Split('|');
            string title = "";
            bool isActive = false;
            var skipChapters = new List<int>();

            if (splitLine[0].StartsWith(ActiveIndicator))
            {
                title = splitLine[0].Substring(ActiveIndicator.Length).Trim();
                isActive = true;
            }
            else if (line.StartsWith(InactiveIndicator))
            {
                title = splitLine[0].Substring(InactiveIndicator.Length).Trim();
            }
            else
            {
                throw new FileLoadException();
            }

            if (splitLine[1].Length > 0)
            {
                splitLine[1] = splitLine[1].Substring(splitLine[1].IndexOf(':') + 1).Trim();
                skipChapters = new List<int>(splitLine[1].Split(',').Select(int.Parse));
            }

            var endChapter = int.Parse(splitLine[2].Substring(splitLine[2].IndexOf(':') + 1).Trim());
            Playlist.Add(new PlaylistItem(title, skipChapters, endChapter, isActive));
        }

        private void OpenMedia()
        {
            if (currentPlayIndex < 0 || currentPlayIndex >= Playlist.Count) return;

            bool playerWasFullScreen = PlayerControl.InFullScreenMode;
            ResetActive();

            try
            {
                var item = Playlist[currentPlayIndex];
                SetPlayStyling();
                dgv_PlayList.CurrentCell = dgv_PlayList.Rows[currentPlayIndex].Cells[1];

                if (File.Exists(item.FilePath))
                {
                    PlayerControl.OpenMedia(item.FilePath);
                }
                else
                {
                    PlayNext();
                }

                if (playerWasFullScreen)
                {
                    PlayerControl.GoFullScreen();
                }

                item.Active = true;
                CurrentItem = item;
                previousChapterPosition = 0;

                Text = PlayerControl.PlayerState + " ─ " + CurrentItem.FilePath;
                ParseChapterInput();
            }
            catch (Exception ex)
            {
                PlayerControl.HandleException(ex);
                PlayNext();
            }

            dgv_PlayList.Invalidate();
        }

        private void SortPlayList(bool ascending = true)
        {
            if (ascending)
            {
                Playlist.Sort();
            }
            else
            {
                Playlist.Sort();
                Playlist.Reverse();
            }

            PopulatePlaylist();
        }

        private void SetPlayStyling()
        {
            foreach (DataGridViewRow r in dgv_PlayList.Rows)
            {
                r.DefaultCellStyle.ForeColor = Color.Black;
                r.Selected = false;
            }

            dgv_PlayList.Rows[currentPlayIndex].DefaultCellStyle.ForeColor = Color.White;
            dgv_PlayList.Rows[currentPlayIndex].Selected = true;
        }

        private void SelectChapter(int chapterNum)
        {
            if (PlayerControl.PlayerState == PlayerState.Closed) return;

            var chapters = GetChapters();

            if (chapters.ElementAt(chapterNum) == null) return;
            PlayerControl.SeekMedia(chapters.ElementAt(chapterNum).Position);
            PlayerControl.ShowOsdText(chapters.ElementAt(chapterNum).Name);
        }

        private int GetChapterIndexByPosition(long position)
        {
            int currentChapterIndex = 0;

            foreach (var c in GetChapters().Where(c => c != null))
            {
                currentChapterIndex++;
                if (c.Position != position) continue;
                return currentChapterIndex;
            }

            return 0;
        }

        private IEnumerable<Chapter> GetChapters()
        {
            return PlayerControl.Chapters.OrderBy(chapter => chapter.Position);
        }

        private void ResetActive()
        {
            foreach (var item in Playlist)
            {
                item.Active = false;
            }
        }

        private void AddFilesToPlaylist()
        {
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            var fileNames = openFileDialog.FileNames;
            AddFiles(fileNames);
        }

        private void RemoveSelectedItems()
        {
            var rowIndexes = new List<int>();

            try
            {
                if (Playlist.Count <= 0) return;

                rowIndexes.AddRange(from DataGridViewRow r in dgv_PlayList.SelectedRows select r.Index);

                foreach (int index in rowIndexes.OrderByDescending(v => v))
                {
                    if (index == currentPlayIndex)
                    {
                        CloseMedia();
                    }

                    Playlist.RemoveAt(index);
                }

                PopulatePlaylist();
            }
            catch (Exception ex)
            {
                PlayerControl.HandleException(ex);
            }
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            AddFilesToPlaylist();
        }

        private void ButtonDelClick(object sender, EventArgs e)
        {
            RemoveSelectedItems();
        }

        private void ButtonNewClick(object sender, EventArgs e)
        {
            NewPlaylist();
        }

        private void ButtonOpenClick(object sender, EventArgs e)
        {
            OpenPlaylist();
        }

        private void ButtonSaveClick(object sender, EventArgs e)
        {
            SavePlaylist();
        }

        private void ButtonLeftClick(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        private void ButtonRightClick(object sender, EventArgs e)
        {
            PlayNext();
        }

        private void ButtonSortAscendingClick(object sender, EventArgs e)
        {
            SortPlayList();
        }

        private void ButtonSortDescendingClick(object sender, EventArgs e)
        {
            SortPlayList(false);
        }

        private void ButtonSettingsClick(object sender, EventArgs e)
        {
            playListUi.ShowConfigDialog(this);
        }

        private void FormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                Hide();
            }
        }

        private void NotifyPlaylistChanged()
        {
            if (PlaylistChanged != null)
                PlaylistChanged(this, null);
        }
    }

    public class PlaylistItem : IComparable<PlaylistItem>
    {
        public string FilePath { get; set; }
        public bool Active { get; set; }
        public bool HasChapter { get; set; }
        public List<int> SkipChapters { get; set; }
        public int EndChapter { get; set; }

        public PlaylistItem(string filePath, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            FilePath = filePath;
            Active = isActive;
        }

        public PlaylistItem(string filePath, List<int> skipChapter, int endChapter, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            FilePath = filePath;
            Active = isActive;
            SkipChapters = skipChapter;
            EndChapter = endChapter;
            HasChapter = true;
        }

        public override string ToString()
        {
            if (HasChapter)
            {
                return Path.GetFileName(FilePath) + " | SkipChapter: " + String.Join(",", SkipChapters) + " | EndChapter: " + EndChapter ?? "???";
            }

            return Path.GetFileName(FilePath) ?? "???";
        }

        public int CompareTo(PlaylistItem other)
        {
            return String.CompareOrdinal(FilePath, other.FilePath);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip)]
    public class ButtonStripItem : ToolStripControlHostProxy
    {
        public ButtonStripItem()
            : base(CreateButtonInstance())
        {
        }

        private static Button CreateButtonInstance()
        {
            var b = new Button {BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat};
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            return b;
        }
    }

    public class ToolStripControlHostProxy : ToolStripControlHost
    {
        public ToolStripControlHostProxy()
            : base(new Control())
        {
        }

        public ToolStripControlHostProxy(Control c)
            : base(c)
        {
        }
    }

    public class FormEx : Form
    {
        private float m_ScaleFactorHeight = -1f;
        private float m_ScaleFactorWidth = -1f;

        protected SizeF ScaleFactor { get; private set; }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            if (!(m_ScaleFactorWidth < 0 || m_ScaleFactorHeight < 0))
                return;

            if (m_ScaleFactorWidth < 0 && specified.HasFlag(BoundsSpecified.Width))
            {
                m_ScaleFactorWidth = factor.Width;
            }
            if (m_ScaleFactorHeight < 0 && specified.HasFlag(BoundsSpecified.Height))
            {
                m_ScaleFactorHeight = factor.Height;
            }

            if (m_ScaleFactorWidth < 0 || m_ScaleFactorHeight < 0)
                return;

            ScaleFactor = new SizeF(m_ScaleFactorWidth, m_ScaleFactorHeight);
        }
    }
}
