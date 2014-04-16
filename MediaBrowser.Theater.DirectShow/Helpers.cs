using MediaBrowser.Theater.Interfaces.Configuration;
using System;
using System.Management;

namespace MediaBrowser.Theater.DirectShow
{
    public static class VideoConfigurationUtils
    {
        //we only need to do this once per run
        private static string _gpuModel = String.Empty;

        public static string GpuModel
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_gpuModel))
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

        public static LAVHWAccel GetHwaMode(VideoConfiguration config, bool preferDXVA2)
        {
            if (config.HwaMode > -1)
                return (LAVHWAccel)config.HwaMode;
            else if (preferDXVA2)
                return LAVHWAccel.DXVA2Native;
            else
            {
                if (GpuModel.IndexOf("Intel") > -1)
                    return LAVHWAccel.QuickSync;
                else
                    return LAVHWAccel.DXVA2CopyBack;
            }
        }

        public static int GetHwaResolutions(VideoConfiguration config)
        {
            if (config.HwaResolution > -1)
                return config.HwaResolution;

            if (GpuModel.IndexOf("Intel") > -1)
                return 7; // SD + HD + UHD
            else
                return 3; // SD + HD;
        }
    }
}
