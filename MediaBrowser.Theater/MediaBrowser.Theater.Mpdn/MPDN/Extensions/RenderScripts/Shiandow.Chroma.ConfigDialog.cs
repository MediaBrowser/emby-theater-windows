using System;
using System.Linq;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Shiandow.Chroma
    {
        public partial class ChromaScalerConfigDialog : ChromaScalerConfigDialogBase
        {
            private bool m_SettingPreset;

            public ChromaScalerConfigDialog()
            {
                InitializeComponent();

                var descs = EnumHelpers.GetDescriptions<Presets>().Where(d => d != "Custom");
                foreach (var desc in descs)
                {
                    PresetBox.Items.Add(desc);
                }

                PresetBox.SelectedIndex = (int) Presets.Custom;
            }

            protected override void LoadSettings()
            {
                BSetter.Value = (Decimal) Settings.B;
                CSetter.Value = (Decimal) Settings.C;
                PresetBox.SelectedIndex = (int) Settings.Preset;
            }

            protected override void SaveSettings()
            {
                Settings.B = (float) BSetter.Value;
                Settings.C = (float) CSetter.Value;
                Settings.Preset = (Presets) PresetBox.SelectedIndex;
            }

            private void ValueChanged(object sender, EventArgs e)
            {
                if (m_SettingPreset)
                    return;

                PresetBox.SelectedIndex = (int) Presets.Custom;
            }

            private void SelectedIndexChanged(object sender, EventArgs e)
            {
                var index = PresetBox.SelectedIndex;
                if (index < 0)
                    return;

                m_SettingPreset = true;
                BSetter.Value = (Decimal) BicubicChroma.B_CONST[index];
                CSetter.Value = (Decimal) BicubicChroma.C_CONST[index];
                m_SettingPreset = false;
            }
        }

        public class ChromaScalerConfigDialogBase : ScriptConfigDialog<BicubicChroma>
        {
        }
    }
}
