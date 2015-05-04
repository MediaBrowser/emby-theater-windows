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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class OpenSubtitlesExtension : PlayerExtension<OpenSubtitlesSettings, OpenSubtitlesConfigDialog>
    {
        private SubtitleDownloader m_Downloader;
        private readonly OpenSubtitlesForm m_Form = new OpenSubtitlesForm();

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("ef5a12c8-246f-41d5-821e-fefdc442b0ea"),
                    Name = "OpenSubtitles",
                    Description = "Download automatically subtitles from OpenSubtitles"
                };
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            m_Downloader = new SubtitleDownloader("MPC-HC", "mpc-hc");
            PlayerControl.MediaLoading += MediaLoading;
        }

        public override void Destroy()
        {
            base.Destroy();
            PlayerControl.MediaLoading -= MediaLoading;
        }


        private void MediaLoading(object sender, MediaLoadingEventArgs e)
        {
            if (!Settings.EnableAutoDownloader)
                return;
            try
            {
                List<Subtitle> subList;
                using (new HourGlass())
                {
                    subList = m_Downloader.GetSubtitles(e.Filename);
                }
                if (subList == null || subList.Count == 0)
                {
                    MessageBox.Show("No Subtitles found.");
                    return;
                }
                subList.Sort((a, b) => a.Lang.CompareTo(b.Lang));
                if (Settings.PreferedLanguage != null)
                {
                    var filteredSubList = subList.FindAll((sub) => sub.Lang.Contains(Settings.PreferedLanguage));
                    if (filteredSubList.Count > 0)
                    {
                        subList = filteredSubList;
                    }
                }

                m_Form.SetSubtitles(subList);
                m_Form.ShowDialog(PlayerControl.Form);
            }
            catch (InternetConnectivityException)
            {
                MessageBox.Show("MPDN can't access OpenSubtitles.org");
            }
            catch (Exception)
            {
                MessageBox.Show("No Subtitles found.");
            }

        }
        public override IList<Verb> Verbs
        {
            get { return new Verb[0]; }
        }

        public class HourGlass : IDisposable
        {
            private const int SETCURSOR = 0x0020;

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

            public HourGlass()
            {
                Enabled = true;
            }

            public void Dispose()
            {
                Enabled = false;
            }

            public static bool Enabled
            {
                get { return Application.UseWaitCursor; }
                set
                {
                    if (value == Application.UseWaitCursor)
                        return;

                    Application.UseWaitCursor = value;
                    var f = Form.ActiveForm;
                    if (f != null)
                    {
                        SendMessage(f.Handle, SETCURSOR, f.Handle.ToInt32(), 1);
                    }
                }
            }
        }
    }

    public class OpenSubtitlesSettings
    {
        public OpenSubtitlesSettings()
        {
            EnableAutoDownloader = false;
            if (CultureInfo.CurrentUICulture.Parent != null)
            {
                PreferedLanguage = CultureInfo.CurrentUICulture.Parent.EnglishName;
            }
            else
            {
                PreferedLanguage = null;
            }
        }

        public bool EnableAutoDownloader { get; set; }
        public string PreferedLanguage { get; set; }
    }
}
