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

namespace Mpdn.PlayerExtensions
{
    namespace GitHub
    {
        public partial class DisplayChangerConfigDialog : DisplayChangerConfigBase
        {
            public DisplayChangerConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                checkBoxActivate.Checked = Settings.Activate;
                checkBoxRestore.Checked = Settings.Restore;
                checkBoxRestoreExit.Checked = Settings.RestoreOnExit;
                checkBoxHighestRate.Checked = Settings.HighestRate;
                checkBoxRestricted.Checked = Settings.Restricted;
                textBoxVideoTypes.Text = Settings.VideoTypes;
            }

            protected override void SaveSettings()
            {
                Settings.Activate = checkBoxActivate.Checked;
                Settings.Restore = checkBoxRestore.Checked;
                Settings.RestoreOnExit = checkBoxRestoreExit.Checked;
                Settings.HighestRate = checkBoxHighestRate.Checked;
                Settings.Restricted = checkBoxRestricted.Checked;
                Settings.VideoTypes = textBoxVideoTypes.Text;
            }
        }

        public class DisplayChangerConfigBase : ScriptConfigDialog<DisplayChangerSettings>
        {
        }
    }
}
