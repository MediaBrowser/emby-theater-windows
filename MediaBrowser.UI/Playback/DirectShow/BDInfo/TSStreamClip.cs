using System;
using System.Collections.Generic;
using System.Text;

namespace BDInfo
{
    public class TSStreamClip
    {
        public int AngleIndex = 0;
        public string Name;
        public double TimeIn;
        public double TimeOut;
        public double RelativeTimeIn;
        public double RelativeTimeOut;
        public double Length;

        public ulong FileSize = 0;
        public ulong PayloadBytes = 0;
        public ulong PacketCount = 0;
        public double PacketSeconds = 0;

        public List<double> Chapters = new List<double>();

        public TSStreamFile StreamFile = null;
        public TSStreamClipFile StreamClipFile = null;

        public TSStreamClip(
            TSStreamFile streamFile,
            TSStreamClipFile streamClipFile)
        {
            Name = streamFile.Name;
            StreamFile = streamFile;
            StreamClipFile = streamClipFile;
            FileSize = (ulong)StreamFile.FileInfo.Length;
        }

        public ulong PacketSize
        {
            get
            {
                return PacketCount * 192;
            }
        }

        public ulong PacketBitRate
        {
            get
            {
                if (PacketSeconds > 0)
                {
                    return (ulong)Math.Round(((PacketSize * 8.0) / PacketSeconds));
                }
                return 0;
            }
        }

        public bool IsCompatible(TSStreamClip clip)
        {
            foreach (TSStream stream1 in StreamFile.Streams.Values)
            {
                if (clip.StreamFile.Streams.ContainsKey(stream1.PID))
                {
                    TSStream stream2 = clip.StreamFile.Streams[stream1.PID];
                    if (stream1.StreamType != stream2.StreamType)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
