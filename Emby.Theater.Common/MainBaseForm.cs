using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emby.Theater.Common
{
    public class MainBaseForm : Form
    {
        public event EventHandler GraphNotify = null;
        public event EventHandler DvdEvent = null;

        public const int WM_APP = 0x8000;
        public const int WM_GRAPH_NOTIFY = WM_APP + 1;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_DVD_EVENT = 0x00008002;
        const int WM_APPCOMMAND = 0x319;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_GRAPH_NOTIFY:
                    {
                        EventHandler eh = GraphNotify;
                        if (eh != null)
                            eh(this, new EventArgs());
                    }
                    break;
                case WM_DVD_EVENT:
                    {
                        EventHandler eh = DvdEvent;
                        if (eh != null)
                            eh(this, new EventArgs());
                    }
                    break;
                case WM_APPCOMMAND:
                    {
                        int cmd = (m.LParam.ToInt32() / 65536);
                        var appCmd = MapAppCommand(cmd);
                        if (appCmd.HasValue)
                        {
                            OnAppCommand(appCmd.Value);
                        }
                        break;
                    }
                default:
                    break;
            }

            base.WndProc(ref m);
        }

        private AppCommand? MapAppCommand(int cmd)
        {
            AppCommand? appCommand = null;

            // dont use exception handling to exclude most frequent appCommand, its very slow
            // use an excludion test first that will catch most of teh cases
            if (cmd >= (int)AppCommand.APPCOMMAND_BROWSER_BACKWARD || cmd <= (int)AppCommand.APPCOMMAND_ASP_TOGGLE)
            {
                try
                {
                    appCommand = (AppCommand)cmd;
                }
                catch (Exception)
                {
                    // not our app command
                }
            }

            return appCommand;
        }

        protected virtual void OnAppCommand(AppCommand cmd)
        {
            
        }
    }
}
