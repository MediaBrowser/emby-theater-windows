using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaBrowser.UI
{
    public partial class HiddenForm : Form
    {
        private const int WM_APP = 0x8000;
        private const int WM_GRAPHNOTIFY = WM_APP + 1;
        
        public HiddenForm()
        {
            InitializeComponent();
        }

        public Action OnWMGRAPHNOTIFY { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GRAPHNOTIFY)
            {
                if (OnWMGRAPHNOTIFY != null)
                {
                    OnWMGRAPHNOTIFY();
                }
            }
            
            base.WndProc(ref m);
        }
    }
}
