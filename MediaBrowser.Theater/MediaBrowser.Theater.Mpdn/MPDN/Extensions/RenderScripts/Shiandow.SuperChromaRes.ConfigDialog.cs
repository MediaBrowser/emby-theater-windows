using System;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Shiandow.SuperRes
    {
        public partial class SuperChromaResConfigDialog : SuperChromaResConfigDialogBase
        {
            public SuperChromaResConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                PassesSetter.Value = (Decimal)Settings.Passes;
                StrengthSetter.Value = (Decimal)Settings.Strength;
                SharpnessSetter.Value = (Decimal)Settings.Sharpness;
                AntiAliasingSetter.Value = (Decimal)Settings.AntiAliasing;
                AntiRingingSetter.Value = (Decimal)Settings.AntiRinging;
                SoftnessSetter.Value = (Decimal)Settings.Softness;
            }

            protected override void SaveSettings()
            {
                Settings.Passes = (int)PassesSetter.Value;
                Settings.Strength = (float)StrengthSetter.Value;
                Settings.Sharpness = (float)SharpnessSetter.Value;
                Settings.AntiAliasing = (float)AntiAliasingSetter.Value;
                Settings.AntiRinging = (float)AntiRingingSetter.Value;
                Settings.Softness = (float)SoftnessSetter.Value;
            }

            private void ValueChanged(object sender, EventArgs e)
            {

            }
        }

        public class SuperChromaResConfigDialogBase : ScriptConfigDialog<SuperChromaRes>
        {
        }
    }
}
