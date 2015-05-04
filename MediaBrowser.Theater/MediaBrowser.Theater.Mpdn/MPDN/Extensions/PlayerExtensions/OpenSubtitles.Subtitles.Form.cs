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
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.GitHub
{
    public partial class OpenSubtitlesForm : Form
    {
        private Subtitle m_SelectedSub;

        public OpenSubtitlesForm()
        {
            InitializeComponent();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            CancelButton = btnCancel;
            FormClosed += OpenSubtitlesForm_FormClosed;
        }

        private void OpenSubtitlesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_SelectedSub = null;
            subtitleBindingSource.DataSource = typeof (Subtitle);
        }

        public void SetSubtitles(List<Subtitle> subtitles)
        {
            subtitleBindingSource.DataSource = subtitles;
        }

        private void OpenSubtitles_Load(object sender, EventArgs e)
        {
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                m_SelectedSub.Save();
                Close();
            }
            catch (InternetConnectivityException)
            {
                MessageBox.Show("MPDN can't access OpenSubtitles.org");
            }
            catch (Exception)
            {
                MessageBox.Show("Can't download the selected subtitle.");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;
            m_SelectedSub = (Subtitle) dataGridView1.SelectedRows[0].DataBoundItem;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DownloadButton_Click(sender, e);
        }

    }
}
