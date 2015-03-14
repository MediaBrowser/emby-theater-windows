using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class Navigation : PlayerExtension
    {
        private readonly string[] m_FileExtensions =
        {
            ".mkv", ".mp4", ".m4v", ".mp4v", ".3g2", ".3gp2", ".3gp", ".3gpp",
            ".mov", ".m2ts", ".ts", ".asf", ".wma", ".wmv", ".wm", ".asx",
            "*.wax", "*.wvx", "*.wmx", ".wpl", ".dvr-ms", ".avi",
            ".mpg", ".mpeg", ".m1v", ".mp2", ".mp3", ".mpa", ".mpe", ".m3u", ".wav",
            ".mid", ".midi", ".rmi"
        };

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("79FFF20D-785B-497C-9716-066787F2A3AC"),
                    Name = "Navigation",
                    Description = "Adds shortcuts for rewinding / forwarding playback"
                };
            }
        }

        public override IList<Verb> Verbs
        {
            get
            {
                return new[]
                {
                    GetVerb("Forward (5 seconds)", "Right", Jump(5)),
                    GetVerb("Backward (5 seconds)", "Left", Jump(-5)),
                    GetVerb("Forward (1 frame)", "Ctrl+Right", StepFrame()),
                    GetVerb("Backward (1 frame)", "Ctrl+Left", JumpFrame(-1)),
                    GetVerb("Forward (30 seconds)", "Ctrl+Shift+Right", Jump(30)),
                    GetVerb("Backward (30 seconds)", "Ctrl+Shift+Left", Jump(-30)),
                    GetVerb("Play next chapter", "Shift+Right", PlayChapter(true)),
                    GetVerb("Play previous chapter", "Shift+Left", PlayChapter(false)),
                    GetVerb("Play next file in folder", "Ctrl+PageDown", PlayFileInFolder(true)),
                    GetVerb("Play previous file in folder", "Ctrl+PageUp", PlayFileInFolder(false))
                };
            }
        }

        private static Verb GetVerb(string menuItemText, string shortCutString, Action action)
        {
            return new Verb(Category.Play, "Navigation", menuItemText, shortCutString, string.Empty, action);
        }

        private Action PlayChapter(bool next)
        {
            return () => SelectChapter(next);
        }

        private void SelectChapter(bool next)
        {
            if (PlayerControl.PlayerState == PlayerState.Closed)
                return;

            var chapters = PlayerControl.Chapters.OrderBy(chapter => chapter.Position);
            var pos = PlayerControl.MediaPosition;
            var nextChapter = next
                ? chapters.SkipWhile(chapter => chapter.Position < pos+1).FirstOrDefault()
                : chapters.TakeWhile(chapter => chapter.Position < Math.Max(pos-1000000, 0)).LastOrDefault();

            if (nextChapter != null)
            {
                PlayerControl.SeekMedia(nextChapter.Position);
                PlayerControl.ShowOsdText(nextChapter.Name);
            }
        }

        private Action PlayFileInFolder(bool next)
        {
            return () => PlayFile(next);
        }

        private void PlayFile(bool next)
        {
            if (PlayerControl.PlayerState == PlayerState.Closed)
                return;

            var mediaPath = PlayerControl.MediaFilePath;
            var mediaDir = GetDirectoryName(mediaPath);
            var mediaFiles = GetMediaFiles(mediaDir);
            var nextFile = next
                ? mediaFiles.SkipWhile(file => file != mediaPath).Skip(1).FirstOrDefault()
                : mediaFiles.TakeWhile(file => file != mediaPath).LastOrDefault();
            if (nextFile != null)
            {
                PlayerControl.OpenMedia(nextFile);
            }
        }

        private static string GetDirectoryName(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            return Path.GetDirectoryName(path) ?? Path.GetPathRoot(path);
        }

        private IEnumerable<string> GetMediaFiles(string mediaDir)
        {
            var files = Directory.EnumerateFiles(mediaDir)
                .OrderBy(filename => filename).Where(file => m_FileExtensions.Contains(Path.GetExtension(file)));
            return files;
        }

        private Action StepFrame()
        {
            return delegate
            {
                if (PlayerControl.PlayerState == PlayerState.Closed)
                    return;

                PlayerControl.StepMedia();
            };
        }

        private Action JumpFrame(int frames)
        {
            return delegate
            {
                if (PlayerControl.PlayerState == PlayerState.Closed)
                    return;

                PlayerControl.PauseMedia(false);
                var pos = PlayerControl.MediaPosition;
                var nextPos = pos + (long) Math.Round(frames*PlayerControl.VideoInfo.AvgTimePerFrame);
                nextPos = Math.Max(0, Math.Min(PlayerControl.MediaDuration, nextPos));
                PlayerControl.SeekMedia(nextPos);
            };
        }

        private Action Jump(float time)
        {
            return delegate
            {
                if (PlayerControl.PlayerState == PlayerState.Closed)
                    return;

                var pos = PlayerControl.MediaPosition;
                var nextPos = pos + (long) Math.Round(time*1000*1000);
                nextPos = Math.Max(0, Math.Min(PlayerControl.MediaDuration, nextPos));
                PlayerControl.SeekMedia(nextPos);
            };
        }
    }
}