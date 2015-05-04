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
                DetailSetter.Value = (Decimal)Settings.detaillevel;

                UpdateGui();
            }

            protected override void SaveSettings()
            {
                Settings.maxbitdepth = (int)MaxBitdepthSetter.Value;
                Settings.threshold = (float)ThresholdSetter.Value;
                Settings.detaillevel = (int)DetailSetter.Value;
            }

            private void ValueChanged(object sender, EventArgs e)
            {
                UpdateGui();
            }

            private void UpdateGui()
            {
            }
        }

        public class DebandConfigDialogBase : ScriptConfigDialog<Deband>
        {
        }
    }
}
