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
using Mpdn.RenderScript.Shiandow.NNedi3.Filters;

namespace Mpdn.RenderScript
{
    namespace Shiandow.NNedi3
    {
        public partial class NNedi3ConfigDialog : NediConfigDialogBase
        {
            public NNedi3ConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                comboBoxNeurons.SelectedIndex = (int) Settings.Neurons;
                comboBoxPath.SelectedIndex = (int) Settings.CodePath;
            }

            protected override void SaveSettings()
            {
                Settings.Neurons = (NNedi3Neurons) comboBoxNeurons.SelectedIndex;
                Settings.CodePath = (NNedi3Path) comboBoxPath.SelectedIndex;
            }
        }

        public class NediConfigDialogBase : ScriptConfigDialog<NNedi3>
        {
        }
    }
}
