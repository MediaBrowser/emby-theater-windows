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
using System.Collections.Generic;
using MediaInfoDotNet;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class ViewMediaInfo : PlayerExtension
    {
        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("6FD61379-FF5D-4143-8A7B-97516BB7822F"),
                    Name = "Media Info",
                    Description = "View media info of current file"
                };
            }
        }

        public override IList<Verb> Verbs
        {
            get
            {
                return new[]
                {
                    new Verb(Category.View, string.Empty, "Media Info...", "Ctrl+Shift+I", string.Empty, ShowMediaInfoDialog)
                };
            }
        }

        private void ShowMediaInfoDialog()
        {
            if (PlayerControl.PlayerState == PlayerState.Closed)
                return;

            using (var form = new ViewMediaInfoForm(PlayerControl.MediaFilePath))
            {
                form.ShowDialog(PlayerControl.VideoPanel);
            }
        }
    }
}
