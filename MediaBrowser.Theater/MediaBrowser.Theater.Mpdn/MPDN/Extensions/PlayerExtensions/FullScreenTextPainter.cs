using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class FullScreenTextPainter : IPlayerExtension
    {
        private const int TEXT_HEIGHT = 20;
        private Timer m_Timer;
        private IText m_Text;
        private volatile bool m_FullScreenMode;
        private volatile bool m_SeekBarShown;
        private long m_Position;
        private long m_Duration;
        private Size m_ScreenSize;

        public ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("D24FA2D6-B3BE-40C1-B3A5-25D1639EB994"),
                    Name = "Text Painter",
                    Description = "Paints current media time code on top of seek bar in full screen mode"
                };
            }
        }

        public bool HasConfigDialog()
        {
            return false;
        }

        public void Initialize()
        {
            m_Text = PlayerControl.CreateText("Verdana", TEXT_HEIGHT, TextFontStyle.Regular);
            m_Timer = new Timer {Interval = 100};
            m_Timer.Tick += TimerOnTick;
            m_Timer.Start();

            PlayerControl.VideoPanel.MouseMove += MouseMove;
            PlayerControl.PaintOverlay += OnPaintOverlay;
            PlayerControl.EnteredFullScreenMode += EnteredFullScreenMode;
            PlayerControl.ExitedFullScreenMode += ExitedFullScreenMode;
        }

        public void Destroy()
        {
            PlayerControl.VideoPanel.MouseMove -= MouseMove;
            PlayerControl.PaintOverlay -= OnPaintOverlay;
            PlayerControl.EnteredFullScreenMode -= EnteredFullScreenMode;
            PlayerControl.ExitedFullScreenMode -= ExitedFullScreenMode;

            m_Timer.Dispose();
            m_Text.Dispose();
        }

        public IList<Verb> Verbs
        {
            get { return new Verb[0]; }
        }

        public bool ShowConfigDialog(IWin32Window owner)
        {
            return false;
        }

        private void ExitedFullScreenMode(object sender, EventArgs e)
        {
            m_FullScreenMode = false;
        }

        private void EnteredFullScreenMode(object sender, EventArgs e)
        {
            m_ScreenSize = PlayerControl.VideoPanel.Size;
            m_FullScreenMode = true;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            m_SeekBarShown = e.Y > PlayerControl.VideoPanel.Height - PlayerControl.FullScreenSeekBarHeight;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (PlayerControl.PlayerState == PlayerState.Closed)
                return;

            AtomicWrite(ref m_Position, PlayerControl.MediaPosition);
            AtomicWrite(ref m_Duration, PlayerControl.MediaDuration);
        }

        private static string GetTimeString(long usec)
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(usec/1000d);
            return duration.ToString(@"hh\:mm\:ss\.fff");
        }

        public static long AtomicRead(long target)
        {
            return Interlocked.CompareExchange(ref target, 0, 0);
        }

        public static void AtomicWrite(ref long target, long value)
        {
            Interlocked.Exchange(ref target, value);
        }

        private void OnPaintOverlay(object sender, EventArgs eventArgs)
        {
            // Warning: This is called from a foreign thread

            if (!m_FullScreenMode)
            {
                m_Text.Hide();
                return;
            }

            if (m_SeekBarShown)
            {
                var position = AtomicRead(m_Position);
                var duration = AtomicRead(m_Duration);
                var text = string.Format("{0} / {1}", GetTimeString(position), GetTimeString(duration));
                var width = m_Text.MeasureWidth(text);
                var size = m_ScreenSize;
                var seekBarHeight = PlayerControl.FullScreenSeekBarHeight; // Note: This property is thread safe
                const int rightOffset = 12;
                const int bottomOffset = 1;
                var location =
                    new Point(size.Width - width - rightOffset,
                              size.Height - seekBarHeight - TEXT_HEIGHT - bottomOffset);
                m_Text.Show(text, location, Color.FromArgb(255, 0xBB, 0xBB, 0xBB),
                    Color.FromArgb(255*60/100, Color.Black), new Padding(5, 0, rightOffset, bottomOffset));
            }
            else
            {
                m_Text.Hide();
            }
        }
    }
}
