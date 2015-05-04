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
using Mpdn.Config;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.GitHub
{
    public partial class OpenSubtitlesConfigDialog : OpenSubtitlesConfigBase
    {
        private readonly CultureInfo InvariantCulture;
        public OpenSubtitlesConfigDialog()
        {
            InitializeComponent();
            var listCulture = new List<CultureInfo>();
            listCulture.AddRange(CultureInfo.GetCultures(CultureTypes.NeutralCultures));
            InvariantCulture = listCulture[0];
            listCulture.Remove(InvariantCulture);
            listCulture.Sort((a, b) => a.EnglishName.CompareTo(b.EnglishName));
            listCulture.Insert(0, InvariantCulture);
            cultureBindingSource.DataSource = listCulture;
        }

        protected override void LoadSettings()
        {
            checkBoxEnableAutoDownloader.Checked = Settings.EnableAutoDownloader;
            if (Settings.PreferedLanguage != null)
            {
                comboBoxPrefLanguage.SelectedValue = Settings.PreferedLanguage;
            }
            else
            {
                comboBoxPrefLanguage.SelectedItem = InvariantCulture;
            }
        }

        protected override void SaveSettings()
        {
            Settings.EnableAutoDownloader = checkBoxEnableAutoDownloader.Checked;
            if (comboBoxPrefLanguage.SelectedItem.Equals(InvariantCulture))
            {
                Settings.PreferedLanguage = null;
            }
            else
            {
                Settings.PreferedLanguage = (string)comboBoxPrefLanguage.SelectedValue;
            }
            
        }

        private void comboBoxPrefLanguage_MouseEnter(object sender, System.EventArgs e)
        {
            toolTipComboBox.SetToolTip((Control)sender, "Used to filter the subtitles. If your language is unavailable it will show all the subtitles.");
        }
    }

    public class OpenSubtitlesConfigBase : ScriptConfigDialog<OpenSubtitlesSettings>
    {
    }
}
