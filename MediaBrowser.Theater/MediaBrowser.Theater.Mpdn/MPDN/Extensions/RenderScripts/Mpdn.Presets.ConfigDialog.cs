using System;
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
        public partial class PresetDialog : PresetDialogBase
        {
            public PresetDialog()
            {
                InitializeComponent();

                UpdateButtons();
            }

            public override void Setup(PresetSettings settings)
            {
                base.Setup(settings);

                var renderScripts = PlayerControl.RenderScripts
                    .Where(script => script is IRenderChainUi)
                    .Select(x => (x as IRenderChainUi).CreateNew())
                    .Concat(new[]{RenderChainUi.Identity});

                scriptBox.DataSource = renderScripts.Select(x => new KeyValuePair<string, IRenderScriptUi>(x.Descriptor.Name, x)).ToList();
                scriptBox.DisplayMember = "Key";
                scriptBox.ValueMember = "Value";
            }

            protected override void LoadSettings()
            {
                AddPresetRange(Settings.Options);

                foreach (DataGridViewRow row in presetGrid.Rows)
                {
                    row.Selected = ((row.Tag as RenderScriptPreset) == Settings.SelectedOption);
                    if (row.Selected) presetGrid.CurrentCell = row.Cells[0];
                }

                NameBox.Text = Settings.Name;
                HotkeyBox.Text = Settings.Hotkey;

                UpdateButtons();
            }

            protected override void SaveSettings()
            {
                var presets = from row in presetGrid.Rows.Cast<DataGridViewRow>()
                              let preset = (RenderScriptPreset)row.Tag
                              where preset != null
                              select preset;

                var selectedIndex = -1;
                if (presetGrid.SelectedRows.Count > 0) selectedIndex = presetGrid.SelectedRows[0].Index;

                Settings.Options.Clear();
                Settings.Options.AddRange(presets);
                Settings.SelectedIndex = selectedIndex;

                Settings.Name = NameBox.Text;
                Settings.Hotkey = HotkeyBox.Text;
            }

            private void AddPresetRange(IEnumerable<RenderScriptPreset> presets)
            {
                foreach (var preset in presets) AddPreset(preset);
            }

            private void AddPreset(RenderScriptPreset preset)
            {
                var row = presetGrid.Rows[presetGrid.Rows.Add()];
                InitializeRow(row, preset);
            }

            private void RemovePreset(DataGridViewRow selectedRow)
            {
                var preset = (RenderScriptPreset)selectedRow.Tag;
                var renderScript = preset.Script;

                renderScript.Destroy();

                var index = selectedRow.Index;
                presetGrid.Rows.Remove(selectedRow);

                UpdateButtons();
            }

            private void UpdateButtons()
            {
                RenderScriptPreset selectedPreset = null;
                if (presetGrid.SelectedRows.Count > 0) selectedPreset = (presetGrid.SelectedRows[0].Tag as RenderScriptPreset);

                buttonConfigure.Enabled = selectedPreset != null && selectedPreset.Script.HasConfigDialog();
                menuRemove.Enabled = selectedPreset != null;
                menuConfigure.Enabled = buttonConfigure.Enabled;
            }

            private void SelectedIndexChanged(object sender, EventArgs e)
            {
                UpdateButtons();
            }

            private void ButtonConfigureClick(object sender, EventArgs e)
            {
                if (presetGrid.SelectedRows.Count <= 0)
                    return;

                var row = presetGrid.SelectedRows[0];
                var preset = (RenderScriptPreset)row.Tag;
                if (preset == null) return;

                var script = preset.Script;
                if (script.HasConfigDialog() && script.ShowConfigDialog(Owner))
                    UpdateRow(row);
            }

            private void RemoveSelectedItem(object sender, EventArgs e)
            {
                foreach (DataGridViewRow row in presetGrid.SelectedRows)
                    RemovePreset(row);
            }

            private void InitializeRow(DataGridViewRow row, RenderScriptPreset preset)
            {
                row.Tag = preset;
                UpdateRow(row);
            }

            private void UpdateRow(DataGridViewRow row)
            {
                var preset = row.Tag as RenderScriptPreset;

                var descriptor = preset.Script.Descriptor;
                presetGrid[0, row.Index].Value = preset.Name;
                presetGrid[1, row.Index].Value = descriptor.Name;
                presetGrid[2, row.Index].Value = descriptor.Description;
            }

            private void CellValueChanged(object sender, DataGridViewCellEventArgs e)
            {
                if (0 <= e.RowIndex && e.RowIndex < presetGrid.Rows.Count)
                {
                    var row = presetGrid.Rows[e.RowIndex];
                    var preset = row.Tag as RenderScriptPreset;
                    preset.Name = row.Cells[0].Value as string;

                    UpdateRow(row);
                }

                UpdateButtons();
            }

            private void addButton_Click(object sender, EventArgs e)
            {
                var script = (scriptBox.SelectedValue as IRenderChainUi).CreateNew();

                var preset = new RenderScriptPreset() { Name = "Preset " + presetGrid.Rows.Count, Script = script };
                AddPreset(preset);
            }
        }

        public class PresetDialogBase : ScriptConfigDialog<PresetSettings>
        {
        }
    }
}