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
using System.Globalization;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.GitHub
{
    public partial class GoToTimeForm : Form
    {
        public GoToTimeForm()
        {
            InitializeComponent();
        }

        public long Position
        {
            get
            {
                TimeSpan timespan;
                if (TimeSpan.TryParseExact(textBoxPos.Text, @"hh\:mm\:ss\.fff", CultureInfo.CurrentCulture, out timespan))
                    return (long) timespan.TotalMilliseconds;

                return -1;
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (Position < 0)
            {
                errorProvider.SetError(textBoxPos, "Invalid time");
                DialogResult = DialogResult.None;
            }
            else
            {
                errorProvider.SetError(textBoxPos, "");
            }
        }
    }
}
