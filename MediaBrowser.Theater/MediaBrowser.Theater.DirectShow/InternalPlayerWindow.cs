using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaBrowser.Theater.DirectShow
{
    public partial class InternalPlayerWindow : Form, IInternalPlayerWindow
    {
        private const int WM_APP = 0x8000;
        private const int WM_GRAPHNOTIFY = WM_APP + 1;
        private const int WM_DVD_EVENT = 0x00008002;

        public InternalPlayerWindow()
        {
            InitializeComponent();
        }

        public Form Form { get { return this; } }
        public Size ContentPixelSize { get { return new Size(Width, Height); } }
        public Action OnWMGRAPHNOTIFY { get; set; }
        public Action OnDVDEVENT { get; set; }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg) {
                case WM_GRAPHNOTIFY:
                    if (OnWMGRAPHNOTIFY != null) {
                        OnWMGRAPHNOTIFY();
                    }
                    break;
                case WM_DVD_EVENT:
                    if (OnDVDEVENT != null) {
                        OnDVDEVENT();
                    }
                    break;
            }

            base.WndProc(ref m);
        }
    }
}