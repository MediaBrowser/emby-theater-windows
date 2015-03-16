using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class GoToTime : PlayerExtension
    {
        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("7C3BA1E2-EE7B-47D2-B174-6AE76D65EC04"),
                    Name = "Go To Time",
                    Description = "Jump to a specified timecode in media"
                };
            }
        }

        public override IList<Verb> Verbs
        {
            get
            {
                return new[]
                {
                    new Verb(Category.Play, string.Empty, "Go To...", "Ctrl+G", string.Empty, GotoPosition)
                };
            }
        }

        private void GotoPosition()
        {
            using (var form = new GoToTimeForm())
            {
                if (form.ShowDialog(PlayerControl.VideoPanel) != DialogResult.OK)
                    return;

                if (PlayerControl.PlayerState == PlayerState.Closed)
                    return;

                if (PlayerControl.PlayerState == PlayerState.Stopped)
                {
                    PlayerControl.PauseMedia(false);
                }

                PlayerControl.SeekMedia(form.Position * 1000);
            }
        }
    }
}