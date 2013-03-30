using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BDInfo
{
    public class BDROM
    {
        public DirectoryInfo DirectoryRoot = null;
        public DirectoryInfo DirectoryBDMV = null;
        public DirectoryInfo DirectoryBDJO = null;
        public DirectoryInfo DirectoryCLIPINF = null;
        public DirectoryInfo DirectoryPLAYLIST = null;
        public DirectoryInfo DirectorySTREAM = null;

        public string VolumeLabel = null;
        public ulong Size = 0;
        public bool IsBDPlus = false;
        public bool IsBDJava = false;

        public Dictionary<string, TSPlaylistFile> PlaylistFiles = 
            new Dictionary<string, TSPlaylistFile>();
        public Dictionary<string, TSStreamClipFile> StreamClipFiles =
            new Dictionary<string, TSStreamClipFile>();
        public Dictionary<string, TSStreamFile> StreamFiles = 
            new Dictionary<string, TSStreamFile>();

        public BDROM(string path)
        {
            //
            // Locate BDMV directories.
            //

            DirectoryBDMV = 
                GetDirectoryBDMV(path);
            
            if (DirectoryBDMV == null)
            {
                //throw new Exception("Unable to locate BD structure.");
            }

            DirectoryRoot = 
                DirectoryBDMV.Parent;
            DirectoryBDJO = 
                GetDirectory("BDJO", DirectoryBDMV, 0);
            DirectoryCLIPINF = 
                GetDirectory("CLIPINF", DirectoryBDMV, 0);
            DirectoryPLAYLIST =
                GetDirectory("PLAYLIST", DirectoryBDMV, 0);
            DirectorySTREAM = 
                GetDirectory("STREAM", DirectoryBDMV, 0);

            if (DirectoryCLIPINF == null
                || DirectoryPLAYLIST == null
                || DirectorySTREAM == null)
            {
                //throw new Exception("Unable to locate BD structure.");
            }

            //
            // Initialize basic disc properties.
            //

            VolumeLabel = GetVolumeLabel(DirectoryRoot);
            Size = (ulong)GetDirectorySize(DirectoryRoot);
            
            if (null != GetDirectory("BDSVM", DirectoryRoot, 0))
            {
                IsBDPlus = true;
            }
            if (null != GetDirectory("SLYVM", DirectoryRoot, 0))
            {
                IsBDPlus = true;
            }
            if (null != GetDirectory("ANYVM", DirectoryRoot, 0))
            {
                IsBDPlus = true;
            }

            if (DirectoryBDJO != null &&
                DirectoryBDJO.GetFiles().Length > 0)
            {
                IsBDJava = true;
            }

            //
            // Initialize file lists.
            //

            FileInfo[] files = null;

            files = DirectoryPLAYLIST.GetFiles("*.MPLS");
            foreach (FileInfo file in files)
            {
                PlaylistFiles.Add(
                    file.Name.ToUpper(), new TSPlaylistFile(this, file));
            }

            files = DirectorySTREAM.GetFiles("*.M2TS");
            foreach (FileInfo file in files)
            {
                StreamFiles.Add(
                    file.Name.ToUpper(), new TSStreamFile(file));
            }

            files = DirectoryCLIPINF.GetFiles("*.CLPI");
            foreach (FileInfo file in files)
            {
                StreamClipFiles.Add(
                    file.Name.ToUpper(), new TSStreamClipFile(file));
            }
        }

        public void Scan()
        {
            foreach (TSStreamClipFile streamClipFile in StreamClipFiles.Values)
            {
                streamClipFile.Scan();
            }
            foreach (TSStreamFile streamFile in StreamFiles.Values)
            {
                streamFile.Scan(null, false);
            }
            foreach (TSPlaylistFile playlistFile in PlaylistFiles.Values)
            {
                playlistFile.Scan(StreamFiles, StreamClipFiles);
                playlistFile.ClearBitrates();
            }
        }

        private DirectoryInfo GetDirectoryBDMV(
            string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            while (dir != null)
            {
                if (dir.Name == "BDMV")
                {
                    return dir;
                }
                dir = dir.Parent;
            }

            return GetDirectory("BDMV", new DirectoryInfo(path), 0);
        }

        private DirectoryInfo GetDirectory(
            string name,
            DirectoryInfo dir,
            int searchDepth)
        {
            DirectoryInfo[] children = dir.GetDirectories();
            foreach (DirectoryInfo child in children)
            {
                if (child.Name == name)
                {
                    return child;
                }
            }
            if (searchDepth > 0)
            {
                foreach (DirectoryInfo child in children)
                {
                    GetDirectory(
                        name, child, searchDepth - 1);
                }
            }
            return null;
        }

        private long GetDirectorySize(DirectoryInfo directoryInfo)
        {
            long size = 0;

            FileInfo[] pathFiles = directoryInfo.GetFiles();
            foreach (FileInfo pathFile in pathFiles)
            {
                size += pathFile.Length;
            }

            DirectoryInfo[] pathChildren = directoryInfo.GetDirectories();
            foreach (DirectoryInfo pathChild in pathChildren)
            {
                size += GetDirectorySize(pathChild);
            }

            return size;
        }

        public string GetVolumeLabel(DirectoryInfo dir)
        {
            uint serialNumber = 0;
            uint maxLength = 0;
            uint volumeFlags = new uint();
            StringBuilder volumeLabel = new StringBuilder(256);
            StringBuilder fileSystemName = new StringBuilder(256);
            
            long result = GetVolumeInformation(
                dir.Name,
                volumeLabel,
                (uint)volumeLabel.Capacity,
                ref serialNumber,
                ref maxLength,
                ref volumeFlags,
                fileSystemName,
                (uint)fileSystemName.Capacity);

            string label = volumeLabel.ToString();
            if (label.Length == 0)
            {
                label = dir.Name;
            }

            return label;
        }

        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(
            string PathName, 
            StringBuilder VolumeNameBuffer, 
            uint VolumeNameSize,
            ref uint VolumeSerialNumber,
            ref uint MaximumComponentLength,
            ref uint FileSystemFlags, 
            StringBuilder FileSystemNameBuffer,
            uint FileSystemNameSize);
    }
}
