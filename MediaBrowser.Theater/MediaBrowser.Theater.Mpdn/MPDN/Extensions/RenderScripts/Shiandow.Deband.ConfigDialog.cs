using System;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Shiandow.Deband
    {
        public partial class DebandConfigDialog : DebandConfigDialogBase
        {
            public DebandConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                MaxBitdepthSetter.Value = (Decimal)Settings.maxbitdepth;
                ThresholdSetter.Value = (Decimal)Settings.threshold;
                MarginSetter.Value = (Decimal)Settings.margin;
                AdvancedBox.Checked = Settings.advancedMode;

                UpdateText();
            }

            protected override void SaveSettings()
            {
                Settings.maxbitdepth = (int)MaxBitdepthSetter.Value;
                Settings.threshold = (float)ThresholdSetter.Value;
                Settings.margin = (float)MarginSetter.Value;
                Settings.advancedMode = AdvancedBox.Checked;
            }

            private void ValueChanged(object sender, EventArgs e)
            {
                UpdateText();
            }

            private void UpdateText() 
            {
                var a = (double)ThresholdSetter.Value;
                var b = (double)MarginSetter.Value;
                var x = (10*a + b + Math.Sqrt(36*a*a - 12*a*b + 33*b*b))/16;
                var y = (a + b - x) / b;
                var z = x * y * (3 * y - 2 * y * y) - a;
                MaxErrorLabel.Text = String.Format("(maximum error {0:N2} bit)", z);
            }

            private void AdvancedBox_CheckedChanged(object sender, EventArgs e)
            {
                panel1.Enabled = AdvancedBox.Checked;
            }
        }

        public class DebandConfigDialogBase : ScriptConfigDialog<Deband>
        {
        }
    }
}
