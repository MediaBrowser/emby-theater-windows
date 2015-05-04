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

namespace Mpdn.PlayerExtensions.Playlist
{
    public partial class PlaylistConfigDialog : PlaylistConfigBase
    {
        public PlaylistConfigDialog()
        {
            InitializeComponent();
        }

        protected override void LoadSettings()
        {
            cb_showPlaylistOnStartup.Checked = Settings.ShowPlaylistOnStartup;
            cb_autoplay.Checked = Settings.Autoplay;
            cb_rememberWindowBounds.Checked = Settings.RememberWindowBounds;
            cb_rememberLastPlayedFile.Checked = Settings.RememberLastPlayedFile;
            cb_addFileToPlaylistOnOpen.Checked = Settings.AddFileToPlaylistOnOpen;
        }

        protected override void SaveSettings()
        {
            Settings.ShowPlaylistOnStartup = cb_showPlaylistOnStartup.Checked;
            Settings.Autoplay = cb_autoplay.Checked;
            Settings.RememberWindowBounds = cb_rememberWindowBounds.Checked;
            Settings.RememberLastPlayedFile = cb_rememberLastPlayedFile.Checked;
            Settings.AddFileToPlaylistOnOpen = cb_addFileToPlaylistOnOpen.Checked;
        }
    }

    public class PlaylistConfigBase : ScriptConfigDialog<PlaylistSettings>
    {
        
    }
}
