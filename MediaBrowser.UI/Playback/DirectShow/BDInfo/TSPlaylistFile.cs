#undef DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BDInfo
{
    public class TSPlaylistFile
    {
        private FileInfo FileInfo = null;
        public string FileType = null;
        public bool IsInitialized = false;
        public string Name = null;
        public BDROM BDROM = null;

        public List<double> Chapters = new List<double>();

        public Dictionary<ushort, TSStream> Streams = 
            new Dictionary<ushort, TSStream>();
        public List<TSStreamClip> StreamClips =
            new List<TSStreamClip>();
        public List<Dictionary<ushort, TSStream>> AngleStreams =
            new List<Dictionary<ushort, TSStream>>();
        public int AngleCount = 0;

        public List<TSStream> SortedStreams = 
            new List<TSStream>();
        public List<TSVideoStream> VideoStreams = 
            new List<TSVideoStream>();
        public List<TSAudioStream> AudioStreams = 
            new List<TSAudioStream>();
        public List<TSTextStream> TextStreams = 
            new List<TSTextStream>();
        public List<TSGraphicsStream> GraphicsStreams = 
            new List<TSGraphicsStream>();

        public TSPlaylistFile(
            BDROM bdrom,
            FileInfo fileInfo)
        {
            BDROM = bdrom;
            FileInfo = fileInfo;
            Name = fileInfo.Name.ToUpper();
        }

        public TSPlaylistFile(
            BDROM bdrom,
            string name,
            List<TSStreamClip> clips)
        {
            BDROM = bdrom;
            Name = name;
            foreach (TSStreamClip clip in clips)
            {
                TSStreamClip newClip = new TSStreamClip(
                    clip.StreamFile, clip.StreamClipFile);

                newClip.Name = clip.Name;
                newClip.TimeIn = clip.TimeIn;
                newClip.TimeOut = clip.TimeOut;
                newClip.Length = newClip.TimeOut - newClip.TimeIn;
                newClip.RelativeTimeIn = TotalLength;
                newClip.RelativeTimeOut = newClip.RelativeTimeIn + newClip.Length;
                newClip.Chapters.Add(clip.TimeIn);
                StreamClips.Add(newClip);

                Chapters.Add(newClip.RelativeTimeIn);
            }
            LoadStreamClips();
            IsInitialized = true;
        }

        public override string ToString()
        {
            return Name;
        }

        public ulong FileSize
        {
            get
            {
                ulong size = 0;
                foreach (TSStreamClip clip in StreamClips)
                {
                    size += clip.FileSize;
                }
                return size;
            }
        }
        public double TotalLength
        {
            get
            {
                double length = 0;
                foreach (TSStreamClip clip in StreamClips)
                {
                    if (clip.AngleIndex == 0)
                    {
                        length += clip.Length;
                    }
                }
                return length;
            }
        }

        public double AngleLength
        {
            get
            {
                double length = 0;
                foreach (TSStreamClip clip in StreamClips)
                {
                    length += clip.Length;
                }
                return length;
            }
        }

        public ulong TotalSize
        {
            get
            {
                ulong size = 0;
                foreach (TSStreamClip clip in StreamClips)
                {
                    if (clip.AngleIndex == 0)
                    {
                        size += clip.PacketSize;
                    }
                }
                return size;
            }
        }

        public ulong AngleSize
        {
            get
            {
                ulong size = 0;
                foreach (TSStreamClip clip in StreamClips)
                {
                    size += clip.PacketSize;
                }
                return size;
            }
        }

        public ulong TotalBitRate
        {
            get
            {
                if (TotalLength > 0)
                {
                    return (ulong)Math.Round(((TotalSize * 8.0) / TotalLength));
                }
                return 0;
            }
        }

        public ulong AngleBitRate
        {
            get
            {
                if (AngleLength > 0)
                {
                    return (ulong)Math.Round(((AngleSize * 8.0) / AngleLength));
                }
                return 0;
            }
        }

        public void Scan(
            Dictionary<string, TSStreamFile> streamFiles,
            Dictionary<string, TSStreamClipFile> streamClipFiles)
        {
            FileStream fileStream = null;
            BinaryReader fileReader = null;

            int streamFileCount = 0;

            try
            {
#if DEBUG
                Debug.WriteLine(string.Format(
                    "Scanning {0}...", Name));
#endif
                Streams.Clear();
                StreamClips.Clear();

                fileStream = File.OpenRead(FileInfo.FullName);
                fileReader = new BinaryReader(fileStream);

                byte[] data = new byte[fileStream.Length];
                int dataLength = fileReader.Read(data, 0, data.Length);

                byte[] fileType = new byte[8];
                Array.Copy(data, 0, fileType, 0, fileType.Length);
                
                FileType = ASCIIEncoding.ASCII.GetString(fileType);
                if ((FileType != "MPLS0100" && FileType != "MPLS0200") 
                    /*|| data[45] != 1*/)
                {
                    throw new Exception(string.Format(
                        "Playlist {0} has an unknown file type {1}.",
                        FileInfo.Name, FileType));
                }
#if DEBUG
                Debug.WriteLine(string.Format(
                    "\tFileType: {0}", FileType));
#endif
                int playlistIndex =
                    ((int)data[8] << 24) +
                    ((int)data[9] << 16) +
                    ((int)data[10] << 8) +
                    ((int)data[11]);

                // TODO: Hack for bad TSRemux output.
                int playlistLength = data.Length - playlistIndex - 4;
                int playlistLengthCorrect =
                    ((int)data[playlistIndex] << 24) +
                    ((int)data[playlistIndex + 1] << 16) +
                    ((int)data[playlistIndex + 2] << 8) +
                    ((int)data[playlistIndex + 3]);

                byte[] playlistData = new byte[playlistLength];
                Array.Copy(data, playlistIndex + 4, 
                    playlistData, 0, playlistData.Length);

                streamFileCount =
                    (((int)playlistData[2] << 8) + (int)playlistData[3]);
#if DEBUG
                Debug.WriteLine(string.Format(
                    "\tStreamFileCount: {0}", streamFileCount));
#endif
                List<TSStreamClip> chapterClips = new List<TSStreamClip>();
                int streamFileOffset = 6;
                for (int streamFileIndex = 0; 
                    streamFileIndex < streamFileCount; 
                    streamFileIndex++)
                {
                    byte[] streamFileNameData = new byte[5];
                    Array.Copy(playlistData, streamFileOffset + 2, 
                        streamFileNameData, 0, streamFileNameData.Length);

                    TSStreamFile streamFile = null;
                    string streamFileName = string.Format(
                        "{0}.M2TS",
                        ASCIIEncoding.ASCII.GetString(streamFileNameData));
                    if (streamFiles.ContainsKey(streamFileName))
                    {
                        streamFile = streamFiles[streamFileName];
                    }
                    if (streamFile == null)
                    {
                        throw new Exception(string.Format(
                            "Playlist {0} referenced missing file {1}.",
                            FileInfo.Name, streamFileName));
                    }

                    TSStreamClipFile streamClipFile = null;
                    string streamClipFileName = string.Format(
                        "{0}.CLPI",
                        ASCIIEncoding.ASCII.GetString(streamFileNameData));
                    if (streamClipFiles.ContainsKey(streamClipFileName))
                    {
                        streamClipFile = streamClipFiles[streamClipFileName];
                    }
                    if (streamClipFile == null)
                    {
                        throw new Exception(string.Format(
                            "Playlist {0} referenced missing file {1}.",
                            FileInfo.Name, streamFileName));
                    }
                    
                    byte condition = (byte)
                        (playlistData[streamFileOffset + 12] & 0xF);

                    ulong timeIn =
                        ((ulong)playlistData[streamFileOffset + 14] << 24) +
                        ((ulong)playlistData[streamFileOffset + 15] << 16) +
                        ((ulong)playlistData[streamFileOffset + 16] << 8) +
                        ((ulong)playlistData[streamFileOffset + 17]);

                    ulong timeOut =
                        ((ulong)playlistData[streamFileOffset + 18] << 24) +
                        ((ulong)playlistData[streamFileOffset + 19] << 16) +
                        ((ulong)playlistData[streamFileOffset + 20] << 8) +
                        ((ulong)playlistData[streamFileOffset + 21]);

                    TSStreamClip streamClip = new TSStreamClip(
                        streamFile, streamClipFile);

                    streamClip.TimeIn = (double)timeIn / 45000;
                    streamClip.TimeOut = (double)timeOut / 45000;
                    streamClip.Length = streamClip.TimeOut - streamClip.TimeIn;
                    streamClip.RelativeTimeIn = TotalLength;
                    streamClip.RelativeTimeOut = streamClip.RelativeTimeIn + streamClip.Length;
                    StreamClips.Add(streamClip);
                    chapterClips.Add(streamClip);
#if DEBUG
                    Debug.WriteLine(string.Format(
                        "\t{0} {1} {2} {3}", 
                        streamClip.Name,
                        streamClip.TimeIn.TotalSeconds,
                        streamClip.TimeOut.TotalSeconds,
                        streamClip.Length.TotalSeconds));
#endif
                    if ((playlistData[streamFileOffset + 12] & 0x10) > 0)
                    {
                        int angleCount = playlistData[streamFileOffset + 34];
                        if (angleCount - 1 > AngleCount)
                        {
                            AngleCount = angleCount - 1;
                        }
                        for (int angle = 0; angle < (angleCount - 1); angle++)
                        {
                            byte[] angleFileNameData = new byte[5];
                            int angleOffset = 
                                streamFileOffset + 26 + ((angle + 1) * 10);
                            Array.Copy(playlistData, angleOffset, 
                                angleFileNameData, 0, angleFileNameData.Length);

                            TSStreamFile angleFile = null;
                            string angleFileName = string.Format(
                                "{0}.M2TS",
                                ASCIIEncoding.ASCII.GetString(angleFileNameData));
                            if (streamFiles.ContainsKey(angleFileName))
                            {
                                angleFile = streamFiles[angleFileName];
                            }
                            if (angleFile == null)
                            {
                                throw new Exception(string.Format(
                                    "Playlist {0} referenced missing angle file {1}.",
                                    FileInfo.Name, angleFileName));
                            }

                            TSStreamClipFile angleClipFile = null;
                            string angleClipFileName = string.Format(
                                "{0}.CLPI",
                                ASCIIEncoding.ASCII.GetString(angleFileNameData));
                            if (streamClipFiles.ContainsKey(angleClipFileName))
                            {
                                angleClipFile = streamClipFiles[angleClipFileName];
                            }
                            if (angleClipFile == null)
                            {
                                throw new Exception(string.Format(
                                    "Playlist {0} referenced missing angle file {1}.",
                                    FileInfo.Name, angleClipFileName));
                            }

                            TSStreamClip angleClip =
                                new TSStreamClip(angleFile, angleClipFile);
                            angleClip.AngleIndex = angle + 1;
                            angleClip.TimeIn = streamClip.TimeIn;
                            angleClip.TimeOut = streamClip.TimeOut;
                            angleClip.RelativeTimeIn = streamClip.RelativeTimeIn;
                            angleClip.RelativeTimeOut = streamClip.RelativeTimeOut;
                            angleClip.Length = streamClip.Length;
                            StreamClips.Add(angleClip);
#if DEBUG
                            Debug.WriteLine(string.Format(
                                "\t\t{0}", angleFileName));
#endif
                        }
                    }
                    streamFileOffset += 2 +
                        ((int)playlistData[streamFileOffset] << 8) +
                        ((int)playlistData[streamFileOffset + 1]);
                }

                int chaptersIndex =
                    ((int)data[12] << 24) +
                    ((int)data[13] << 16) +
                    ((int)data[14] << 8) +
                    ((int)data[15]);

                int chaptersLength =
                    ((int)data[chaptersIndex] << 24) +
                    ((int)data[chaptersIndex + 1] << 16) +
                    ((int)data[chaptersIndex + 2] << 8) +
                    ((int)data[chaptersIndex + 3]);

                byte[] chapterData = 
                    new byte[chaptersLength];
                Array.Copy(data, chaptersIndex + 4, 
                    chapterData, 0, chaptersLength);

                int chapterCount = 
                    ((int)chapterData[0] << 8) + chapterData[1];
                int chapterOffset = 2;
                for (int chapterIndex = 0; 
                    chapterIndex < chapterCount; 
                    chapterIndex++)
                {
                    if (chapterData[chapterOffset + 1] == 1)
                    {
                        int streamFileIndex =
                            ((int)chapterData[chapterOffset + 2] << 8) + 
                            chapterData[chapterOffset + 3];

                        TSStreamClip streamClip = chapterClips[streamFileIndex];

                        long chapterTime =
                            ((long)chapterData[chapterOffset + 4] << 24) +
                            ((long)chapterData[chapterOffset + 5] << 16) +
                            ((long)chapterData[chapterOffset + 6] << 8) +
                            ((long)chapterData[chapterOffset + 7]);

                        double chapterSeconds = (double)chapterTime / 45000;
                        double relativeSeconds =
                            chapterSeconds -
                            streamClip.TimeIn +
                            streamClip.RelativeTimeIn;

                        // TODO: Ignore short last chapter?
                        if (TotalLength - relativeSeconds > 1.0)
                        {
                            streamClip.Chapters.Add(chapterSeconds);
                            this.Chapters.Add(relativeSeconds);
                        }
#if DEBUG
                        Debug.WriteLine(string.Format(
                            "\t{0} {1} {2}", 
                            chapterIndex, 
                            streamClip.Name, 
                            chapter.TotalSeconds));
#endif
                    }
                    chapterOffset += 14;
                }
#if DEBUG
                Debug.WriteLine(string.Format(
                    "\tLength: {0}", Length.TotalSeconds));
                Debug.WriteLine(string.Format(
                    "\tAngleLength: {0}", AngleLength.TotalSeconds));
#endif
                LoadStreamClips();
                IsInitialized = true;
            }
            finally
            {
                if (fileReader != null)
                {
                    fileReader.Close();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        private void LoadStreamClips()
        {
            TSStreamClip referenceClip = null;
            double referenceClipLength = 0;
            foreach (TSStreamClip clip in StreamClips)
            {
                if (clip.Length > referenceClipLength)
                {
                    referenceClip = clip;
                    referenceClipLength = clip.Length;
                }
            }
            if (referenceClip == null &&
                StreamClips.Count > 0)
            {
                referenceClip = StreamClips[0];
            }

            foreach (TSStream clipStream
                in referenceClip.StreamClipFile.Streams.Values)
            {
                if (!Streams.ContainsKey(clipStream.PID))
                {
                    TSStream stream = clipStream.Clone();
                    Streams[clipStream.PID] = stream;

                    if (stream.IsVideoStream)
                    {
                        VideoStreams.Add((TSVideoStream)stream);
                    }
                    else if (stream.IsAudioStream)
                    {
                        AudioStreams.Add((TSAudioStream)stream);
                    }
                    else if (stream.IsGraphicsStream)
                    {
                        GraphicsStreams.Add((TSGraphicsStream)stream);
                    }
                    else if (stream.IsTextStream)
                    {
                        TextStreams.Add((TSTextStream)stream);
                    }
                }
            }

            foreach (TSStream clipStream
                in referenceClip.StreamFile.Streams.Values)
            {
                if (Streams.ContainsKey(clipStream.PID))
                {
                    TSStream stream = Streams[clipStream.PID];

                    if (stream.StreamType != clipStream.StreamType) continue;

                    if (clipStream.BitRate > stream.BitRate)
                    {
                        stream.BitRate = clipStream.BitRate;
                    }
                    stream.IsVBR = clipStream.IsVBR;

                    if (stream.IsVideoStream &&
                        clipStream.IsVideoStream)
                    {
                        ((TSVideoStream)stream).EncodingProfile =
                            ((TSVideoStream)clipStream).EncodingProfile;
                    }
                    else if (stream.IsAudioStream &&
                        clipStream.IsAudioStream)
                    {
                        TSAudioStream audioStream = (TSAudioStream)stream;
                        TSAudioStream clipAudioStream = (TSAudioStream)clipStream;

                        if (clipAudioStream.ChannelCount > audioStream.ChannelCount)
                        {
                            audioStream.ChannelCount = clipAudioStream.ChannelCount;
                        }
                        if (clipAudioStream.LFE > audioStream.LFE)
                        {
                            audioStream.LFE = clipAudioStream.LFE;
                        }
                        if (clipAudioStream.SampleRate > audioStream.SampleRate)
                        {
                            audioStream.SampleRate = clipAudioStream.SampleRate;
                        }
                        if (clipAudioStream.BitDepth > audioStream.BitDepth)
                        {
                            audioStream.BitDepth = clipAudioStream.BitDepth;
                        }
                        if (clipAudioStream.DialNorm < audioStream.DialNorm)
                        {
                            audioStream.DialNorm = clipAudioStream.DialNorm;
                        }
                        if (clipAudioStream.AudioMode != TSAudioMode.Unknown)
                        {
                            audioStream.AudioMode = clipAudioStream.AudioMode;
                        }
                        if (clipAudioStream.CoreStream != null &&
                            audioStream.CoreStream == null)
                        {
                            audioStream.CoreStream = (TSAudioStream)
                                clipAudioStream.CoreStream.Clone();
                        }
                    }
                }
            }

            for (int i = 0; i < AngleCount; i++)
            {
                AngleStreams.Add(new Dictionary<ushort, TSStream>());
            }

            //if (!BDInfoSettings.KeepStreamOrder)
            //{
            //    VideoStreams.Sort(CompareVideoStreams);
            //}
            foreach (TSStream stream in VideoStreams)
            {
                SortedStreams.Add(stream);
                for (int i = 0; i < AngleCount; i++)
                {
                    TSStream angleStream = stream.Clone();
                    angleStream.AngleIndex = i + 1;
                    AngleStreams[i][angleStream.PID] = angleStream;
                    SortedStreams.Add(angleStream);
                }
            }

            //if (!BDInfoSettings.KeepStreamOrder)
            //{
            //    AudioStreams.Sort(CompareAudioStreams);
            //}
            foreach (TSStream stream in AudioStreams)
            {
                SortedStreams.Add(stream);
            }

            //if (!BDInfoSettings.KeepStreamOrder)
            //{
            //    GraphicsStreams.Sort(CompareGraphicsStreams);
            //}
            foreach (TSStream stream in GraphicsStreams)
            {
                SortedStreams.Add(stream);
            }

            //if (!BDInfoSettings.KeepStreamOrder)
            //{
            //    TextStreams.Sort(CompareTextStreams);
            //}
            foreach (TSStream stream in TextStreams)
            {
                SortedStreams.Add(stream);
            }
        }

        public void ClearBitrates()
        {
            foreach (TSStreamClip clip in StreamClips)
            {
                clip.PayloadBytes = 0;
                clip.PacketCount = 0;
                clip.PacketSeconds = 0;

                foreach (TSStream stream in clip.StreamFile.Streams.Values)
                {
                    stream.PayloadBytes = 0;
                    stream.PacketCount = 0;
                    stream.PacketSeconds = 0;
                }

                if (clip.StreamFile != null &&
                    clip.StreamFile.StreamDiagnostics != null)
                {
                    clip.StreamFile.StreamDiagnostics.Clear();
                }
            }

            foreach (TSStream stream in SortedStreams)
            {
                stream.PayloadBytes = 0;
                stream.PacketCount = 0;
                stream.PacketSeconds = 0;
            }
        }


        public bool FilterPlaylist = true;  //hard coded setting to filter playlist
        public bool IsValid
        {
            get
            {
                if (!IsInitialized) return false;
                if (FilterPlaylist == true) return true; //if (!BDInfoSettings.FilterPlaylists) return true;  //need this setting!!!!!
                if (TotalLength < 10) return false;

                Dictionary<string, TSStreamClip> clips =
                    new Dictionary<string, TSStreamClip>();
                foreach (TSStreamClip clip in StreamClips)
                {
                    if (!clips.ContainsKey(clip.Name))
                    {
                        clips[clip.Name] = clip;
                    }
                    else return false;
                }
                return true;
            }
        }

        public static int CompareVideoStreams(
            TSVideoStream x, 
            TSVideoStream y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return 1;
            }
            else if (x != null && y == null)
            {
                return -1;
            }
            else
            {
                if (x.Height > y.Height)
                {
                    return -1;
                }
                else if (y.Height > x.Height)
                {
                    return 1;
                }
                else if (x.PID > y.PID)
                {
                    return 1;
                }
                else if (y.PID > x.PID)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static int CompareAudioStreams(
            TSAudioStream x, 
            TSAudioStream y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }
            else
            {
                if (x.ChannelCount > y.ChannelCount)
                {
                    return -1;
                }
                else if (y.ChannelCount > x.ChannelCount)
                {
                    return 1;
                }
                else
                {
                    int sortX = GetStreamTypeSortIndex(x.StreamType);
                    int sortY = GetStreamTypeSortIndex(y.StreamType);

                    if (sortX > sortY)
                    {
                        return -1;
                    }
                    else if (sortY > sortX)
                    {
                        return 1;
                    }
                    else
                    {
                        if (x.LanguageCode == "eng")
                        {
                            return -1;
                        }
                        else if (y.LanguageCode == "eng")
                        {
                            return 1;
                        }
                        else
                        {
                            return string.Compare(
                                x.LanguageName, y.LanguageName);
                        }
                    }
                }
            }
        }

        public static int CompareTextStreams(
            TSTextStream x,
            TSTextStream y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }
            else
            {
                if (x.LanguageCode == "eng")
                {
                    return -1;
                }
                else if (y.LanguageCode == "eng")
                {
                    return 1;
                }
                else
                {
                    if (x.LanguageCode == y.LanguageCode)
                    {
                        if (x.PID > y.PID)
                        {
                            return 1;
                        }
                        else if (y.PID > x.PID)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return string.Compare(
                            x.LanguageName, y.LanguageName);
                    }
                }
            }
        }

        private static int CompareGraphicsStreams(
            TSGraphicsStream x,
            TSGraphicsStream y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }
            else
            {
                int sortX = GetStreamTypeSortIndex(x.StreamType);
                int sortY = GetStreamTypeSortIndex(y.StreamType);

                if (sortX > sortY)
                {
                    return -1;
                }
                else if (sortY > sortX)
                {
                    return 1;
                }
                else if (x.LanguageCode == "eng")
                {
                    return -1;
                }
                else if (y.LanguageCode == "eng")
                {
                    return 1;
                }
                else
                {
                    if (x.LanguageCode == y.LanguageCode)
                    {
                        if (x.PID > y.PID)
                        {
                            return 1;
                        }
                        else if (y.PID > x.PID)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return string.Compare(x.LanguageName, y.LanguageName);
                    }
                }
            }
        }

        private static int GetStreamTypeSortIndex(TSStreamType streamType)
        {
            switch (streamType)
            {
                case TSStreamType.Unknown:
                    return 0;
                case TSStreamType.MPEG1_VIDEO:
                    return 1;
                case TSStreamType.MPEG2_VIDEO:
                    return 2;
                case TSStreamType.AVC_VIDEO:
                    return 3;
                case TSStreamType.VC1_VIDEO:
                    return 4;

                case TSStreamType.MPEG1_AUDIO:
                    return 1;
                case TSStreamType.MPEG2_AUDIO:
                    return 2;
                case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
                    return 3;
                case TSStreamType.DTS_HD_SECONDARY_AUDIO:
                    return 4;
                case TSStreamType.AC3_AUDIO:
                    return 5;
                case TSStreamType.DTS_AUDIO:
                    return 6;
                case TSStreamType.AC3_PLUS_AUDIO:
                    return 7;
                case TSStreamType.DTS_HD_AUDIO:
                    return 8;
                case TSStreamType.AC3_TRUE_HD_AUDIO:
                    return 9;
                case TSStreamType.DTS_HD_MASTER_AUDIO:
                    return 10;
                case TSStreamType.LPCM_AUDIO:
                    return 11;

                case TSStreamType.SUBTITLE:
                    return 1;
                case TSStreamType.INTERACTIVE_GRAPHICS:
                    return 2;
                case TSStreamType.PRESENTATION_GRAPHICS:
                    return 3;

                default:
                    return 0;
            }
        }
    }
}
