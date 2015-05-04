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
using System.Drawing;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mpdn.RenderScript;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Mpdn.Presets
    {
        public partial class PresetGroupDialog : PresetGroupDialogBase
        {
            public PresetGroupDialog()
            {
                InitializeComponent();

                UpdateControls();
            }

            protected override void LoadSettings()
            {
                ReloadSettings();
                
                UpdateControls();
            }

            private void ReloadSettings()
            {
                ScriptBox.DataSource = Settings.Options;
                ScriptBox.DisplayMember = "Name";
                if (0 <= Settings.SelectedIndex && Settings.SelectedIndex < ScriptBox.Items.Count)
                    ScriptBox.SelectedIndex = Settings.SelectedIndex;
                else
                    ScriptBox.SelectedIndex = ScriptBox.Items.Count - 1;

                HotkeyBox.Text = Settings.Hotkey;
            }

            protected override void SaveSettings()
            {
                Settings.SelectedIndex = ScriptBox.SelectedIndex;
                Settings.Hotkey = HotkeyBox.Text;
            }

            private void UpdateControls()
            {
                var preset = (Preset)ScriptBox.SelectedItem;
                
                buttonConfigure.Enabled = preset != null && preset.Script.HasConfigDialog();
                DescriptionLabel.Text = preset != null ? preset.Script.Descriptor.Description : "";
            }

            private void buttonConfigure_Click(object sender, EventArgs e)
            {
                var preset = (Preset)ScriptBox.SelectedItem;
                if (preset != null && preset.Script.HasConfigDialog() && preset.Script.ShowConfigDialog(Owner))
                    UpdateControls();
            }

            private void AdancedMode()
            {
                using (var dialog = new PresetGroupAdvDialog())
                {
                    dialog.Setup(Settings);
                    if (dialog.ShowDialog(Owner) != DialogResult.OK)
                        return;

                    ReloadSettings();
                }
            }

            private void buttonAdv_Click(object sender, EventArgs e)
            {
                AdancedMode();
            }

            private void ScriptBox_SelectedIndexChanged(object sender, EventArgs e)
            {
                UpdateControls();
            }
        }

        public class PresetGroupDialogBase : ScriptConfigDialog<PresetGroup>
        {
        }
    }
}
