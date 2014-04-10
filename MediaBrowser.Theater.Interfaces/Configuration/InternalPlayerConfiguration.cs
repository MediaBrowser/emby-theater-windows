using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;

namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class InternalPlayerConfiguration
    /// </summary>
    public class InternalPlayerConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable reclock].
        /// </summary>
        /// <value><c>true</c> if [enable reclock]; otherwise, <c>false</c>.</value>
        public bool EnableReclock { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable madvr].
        /// </summary>
        /// <value><c>true</c> if [enable madvr]; otherwise, <c>false</c>.</value>
        public bool EnableMadvr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable xy sub filter].
        /// </summary>
        /// <value><c>true</c> if [enable xy sub filter]; otherwise, <c>false</c>.</value>
        public bool EnableXySubFilter { get; set; }

        public VideoConfiguration VideoConfig { get; set; }

        public AudioConfiguration AudioConfig { get; set; }

        public SubtitleConfiguration SubtitleConfig { get; set; }

        public KnownCOMObjectConfiguration COMConfig { get; set; }

        public InternalPlayerConfiguration()
        {
            //set defaults if necessary
            VideoConfig = new VideoConfiguration();
            AudioConfig = new AudioConfiguration();
            SubtitleConfig = new SubtitleConfiguration();
            COMConfig = new KnownCOMObjectConfiguration();
        }
    }
    
    //add configuration values here as necessary
    public class VideoConfiguration
    {
        //we only need to do this once per run
        private static string _gpuModel = string.Empty;
        public static string GpuModel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_gpuModel))
                {
                    //this may not work for multi-GPU systems
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration"))
                    {
                        foreach (ManagementObject mo in searcher.Get())
                        {
                            foreach (PropertyData property in mo.Properties)
                            {
                                if (property.Name == "Description")
                                {
                                    _gpuModel = property.Value.ToString();
                                    break;
                                }
                            }
                            mo.Dispose();
                        }
                    }
                }
                return _gpuModel;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating overridden video HWA mode.
        /// </summary>
        public int HwaMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating resolutions that will be HWA.
        /// </summary>
        /// equivalent to LAVHWResFlag which appears to be a bitwise comparison enum
        public int HwaResolution { get; set; }

        /// <summary>
        /// Gets or sets a value enabling madVR smooth motion.
        /// </summary>
        /// <value><c>true</c> to enable; otherwise, <c>false</c>.</value>
        public bool UseMadVrSmoothMotion { get; set; }

        /// <summary>
        /// Gets or sets madVR smooth motion mode.
        /// </summary>
        /// <value><c>avoidJudder</c>, <c>almostAlways</c> or <c>always</c>.</value>
        public string MadVrSmoothMotionMode { get; set; }

        /// <summary>
        /// Gets or sets video codecs that will be HWA. 
        /// </summary>
        public List<string> HwaEnabledCodecs { get; set; }

        /// <summary>
        /// Gets or sets video codecs that will be enabled. 
        /// </summary>
        public List<string> EnabledCodecs { get; set; }

        public bool ShowTrayIcon { get; set; }

        public bool UseCustomPresenter { get; set; }

        public VideoConfiguration()
        {
            HwaEnabledCodecs = new List<string>();
            EnabledCodecs = new List<string>();

            UseMadVrSmoothMotion = false;
            MadVrSmoothMotionMode = "avoidJudder";

            HwaResolution = -1;
            HwaMode = -1;

            UseCustomPresenter = true;
        }

        public void SetDefaults()
        {
            //if (HwaMode < 0 || HwaResolution < 0)
            //{
            //    if (GpuModel.IndexOf("Intel") > -1)
            //    {
            //        HwaResolution = 7; // SD + HD + UHD
            //        HwaMode = 2; //LAVHWAccel.QuickSync;
            //    }
            //    else
            //    {
            //        HwaResolution = 3; // SD + HD; 
            //        HwaMode = 3; // LAVHWAccel.DXVA2CopyBack;
            //    }
            //}
            //reading through nevcariel's comments it appears that HWA DVD playback can have stability issues
            //and since most any PC should be able to manage it, we're not going to turn it on by default
            //also skip MPEG4 since most GPUs can't HWA and it's buggy
            //the full list of codecs can be had from DirectShowPlayer.GetLAVVideoHwaCodecs for UI config building
            if (HwaEnabledCodecs.Count == 0)
            {
                HwaEnabledCodecs.Add("H264");
                HwaEnabledCodecs.Add("VC1");
                HwaEnabledCodecs.Add("MPEG2");
                //HwaEnabledCodecs.Add("MPEG2DVD");
                //HwaEnabledCodecs.Add("MPEG4");
            }

            if (EnabledCodecs.Count == 0)
            {
                EnabledCodecs.Add("H264");
                EnabledCodecs.Add("VC1");
                EnabledCodecs.Add("MPEG1");
                EnabledCodecs.Add("MPEG2");
                EnabledCodecs.Add("MPEG4");
                EnabledCodecs.Add("MSMPEG4");
                EnabledCodecs.Add("VP8");
                EnabledCodecs.Add("WMV3");
                EnabledCodecs.Add("WMV12");
                EnabledCodecs.Add("MJPEG");
                EnabledCodecs.Add("Theora");
                EnabledCodecs.Add("FLV1");
                EnabledCodecs.Add("VP6");
                EnabledCodecs.Add("SVQ");
                EnabledCodecs.Add("H261");
                EnabledCodecs.Add("H263");
                EnabledCodecs.Add("Indeo");
                EnabledCodecs.Add("TSCC");
                EnabledCodecs.Add("Fraps");
                EnabledCodecs.Add("HuffYUV");
                EnabledCodecs.Add("QTRle");
                EnabledCodecs.Add("DV");
                EnabledCodecs.Add("Bink");
                EnabledCodecs.Add("Smacker");
                EnabledCodecs.Add("RV34");
                EnabledCodecs.Add("Lagarith");
                EnabledCodecs.Add("Camstudio");
                EnabledCodecs.Add("ZLIB");
                EnabledCodecs.Add("QTRpza");
                EnabledCodecs.Add("PNG");
                EnabledCodecs.Add("ProRes");
                EnabledCodecs.Add("UtVideo");
                EnabledCodecs.Add("Dirac");
                EnabledCodecs.Add("DNxHD");
                EnabledCodecs.Add("MSVideo1");
                EnabledCodecs.Add("EightBPS");
                EnabledCodecs.Add("LOCO");
                EnabledCodecs.Add("ZMBV");
                EnabledCodecs.Add("VCR1");
                EnabledCodecs.Add("Snow");
                EnabledCodecs.Add("FFV1");
            }
        }
    }

    public enum BitstreamChoice
    {
        None = 0,
        SPDIF = 1,
        HDMI = 3
    }

    //add configuration values here as necessary
    public class AudioConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable audio bitstreaming].
        /// </summary>
        /// <value><c>true</c> if [enable audio bitstreaming]; otherwise, <c>false</c>.</value>
        public BitstreamChoice AudioBitstreaming { get; set; }

        public int Delay { get; set; }

        public bool EnableAutoSync { get; set; }
        public bool ConvertToStandardLayout { get; set; }
        public bool ExpandMono { get; set; }
        public bool Expand61 { get; set; }
        public bool EnableDRC { get; set; }
        public int DRCLevel { get; set; }
        public bool EnablePCMMixing { get; set; }
        public string MixingEncoding { get; set; }
        public string MixingLayout { get; set; }
        public int MixingSetting { get; set; }
        public double LfeMixingLevel { get; set; }
        public double CenterMixingLevel { get; set; }
        public double SurroundMixingLevel { get; set; }
        public bool ShowTrayIcon { get; set; }

        /// <summary>
        /// Gets or sets audio codecs that will be enabled. 
        /// </summary>
        public List<string> EnabledCodecs { get; set; }

        public AudioConfiguration()
        {
            EnabledCodecs = new List<string>();

            Delay = 0;
            MixingEncoding = "None";
            MixingLayout = "Stereo";
            MixingSetting = 4; //"ClipProtection"
            EnablePCMMixing = false;
            EnableAutoSync = true;
            ConvertToStandardLayout = true;
            Expand61 = false;
            ExpandMono = false;
            EnableDRC = false;
            DRCLevel = 100;
            LfeMixingLevel = 0;
            CenterMixingLevel = 0.7071;
            SurroundMixingLevel = 0.7071;
        }

        public void SetDefaults()
        {
            if (EnabledCodecs.Count == 0)
            {
                EnabledCodecs.Add("AAC");
                EnabledCodecs.Add("AC3");
                EnabledCodecs.Add("EAC3");
                EnabledCodecs.Add("DTS");
                EnabledCodecs.Add("MP2");
                EnabledCodecs.Add("MP3");
                EnabledCodecs.Add("TRUEHD");
                EnabledCodecs.Add("FLAC");
                EnabledCodecs.Add("VORBIS");
                EnabledCodecs.Add("LPCM");
                EnabledCodecs.Add("PCM");
                EnabledCodecs.Add("WAVPACK");
                EnabledCodecs.Add("TTA");
                EnabledCodecs.Add("Cook");
                EnabledCodecs.Add("RealAudio");
                EnabledCodecs.Add("ALAC");
                EnabledCodecs.Add("Opus");
                EnabledCodecs.Add("AMR");
                EnabledCodecs.Add("Nellymoser");
                EnabledCodecs.Add("MSPCM");
                EnabledCodecs.Add("Truespeech");
                EnabledCodecs.Add("TAK");
            }
        }
    }

    //add configuration values here as necessary
    public class SubtitleConfiguration
    {   
        public List<string> ExternalExtensions { get; set; }

        public SubtitleConfiguration()
        {
            ExternalExtensions = new List<string>();
        }

        public void SetDefaults()
        {
            if (ExternalExtensions.Count == 0)
            {
                ExternalExtensions.Add("SRT");
            }
        }
    }

    public class KnownCOMObjectConfiguration
    {
        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        public KnownCOMObjectConfiguration()
        {
            FilterList = new SerializableDictionary<Guid, KnownCOMObject>();
        }

        public SerializableDictionary<Guid, KnownCOMObject> FilterList { get; set;}

        public void SetDefaults()
        {
            if (FilterList.Count == 0)
            {
                //FilterList[new Guid("")] = new KnownFilter("", "", new Guid(""));
                FilterList[new Guid("{171252A0-8820-4AFE-9DF8-5C92B2D66B04}")] = new KnownCOMObject("LAV Splitter", "LAV\\LAVSplitter.ax", new Guid("{171252A0-8820-4AFE-9DF8-5C92B2D66B04}"));
                FilterList[new Guid("{B98D13E7-55DB-4385-A33D-09FD1BA26338}")] = new KnownCOMObject("LAV Splitter Source", "LAV\\LAVSplitter.ax", new Guid("{B98D13E7-55DB-4385-A33D-09FD1BA26338}"));
                FilterList[new Guid("{E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491}")] = new KnownCOMObject("LAV Audio Decoder", "LAV\\LAVAudio.ax", new Guid("{E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491}"));
                FilterList[new Guid("{EE30215D-164F-4A92-A4EB-9D4C13390F9F}")] = new KnownCOMObject("LAV Video Decoder", "LAV\\LAVVideo.ax", new Guid("{EE30215D-164F-4A92-A4EB-9D4C13390F9F}"));
                FilterList[new Guid("{E1A8B82A-32CE-4B0D-BE0D-AA68C772E423}")] = new KnownCOMObject("madVR", "madVR\\madVR.ax", new Guid("{E1A8B82A-32CE-4B0D-BE0D-AA68C772E423}"));
                FilterList[new Guid("{2DFCB782-EC20-4A7C-B530-4577ADB33F21}")] = new KnownCOMObject("XySubFilter", "XySubFilter\\XySubFilter.dll", new Guid("{2DFCB782-EC20-4A7C-B530-4577ADB33F21}"));
                FilterList[new Guid("{5325DF1C-6F10-4292-B8FB-BE855F99F88A}")] = new KnownCOMObject("EVR Presenter (babgvant)", "babgvant\\EVRPresenter.dll", new Guid("{5325DF1C-6F10-4292-B8FB-BE855F99F88A}"));
            }
        }

        public static bool IsGuid(string candidate, out Guid output)
        {
            bool isValid = false;
            output = Guid.Empty;

            if (candidate != null)
            {
                if (isGuid.IsMatch(candidate))
                {
                    output = new Guid(candidate);
                    isValid = true;
                }
            }
            return isValid;
        }
    }

    public class KnownCOMObject
    {
        public string ObjectName { get; set; }
        public string ObjectPath { get; set; }
        public Guid Clsid { get; set; }

        public KnownCOMObject()
        {

        }

        public KnownCOMObject(string name, string path, Guid clsid)
        {
            this.ObjectName = name;
            this.ObjectPath = path;
            this.Clsid = clsid;
        }


    }
}
