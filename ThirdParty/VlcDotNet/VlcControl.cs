using System;
using System.ComponentModel;
using System.Windows.Forms;
using Vlc.DotNet.Core;

namespace Vlc.DotNet.Forms
{
    public sealed partial class VlcControl
    {
        /// <summary>
        /// Constructor of VlcControl
        /// </summary>
        public VlcControl()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
            if (!VlcContext.IsInitialized)
                VlcContext.Initialize();
            VlcContext.HandleManager.MediaPlayerHandles[this] =
                VlcContext.InteropManager.MediaPlayerInterops.NewInstance.Invoke(
                    VlcContext.HandleManager.LibVlcHandle);
            AudioProperties = new VlcAudioProperties(this);
            VideoProperties = new VlcVideoProperties(this);
            Medias = new VlcMediaListPlayer(this);
            LogProperties = new VlcLogProperties();
            AudioOutputDevices = new VlcAudioOutputDevices();

            EventsHelper.ExecuteRaiseEventDelegate =
                delegate(Delegate singleInvoke, object sender, object arg)
                {
                    var syncInvoke = singleInvoke.Target as ISynchronizeInvoke;
                    if (syncInvoke == null)
                    {
                        singleInvoke.DynamicInvoke(new[] { sender, arg });
                        return;
                    }
                    try
                    {
                        if (syncInvoke.InvokeRequired)
                            syncInvoke.Invoke(singleInvoke, new[] { sender, arg });
                        else
                            singleInvoke.DynamicInvoke(sender, arg);
                    }
                    catch (ObjectDisposedException)
                    {
                        //Because IsDisposed was true and IsDisposed could be false now...
                    }
                };

            InitEvents();
        }

        /// <summary>
        /// Sets the handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        public void SetHandle(IntPtr handle)
        {
            VlcContext.InteropManager.MediaPlayerInterops.SetHwnd.Invoke(VlcContext.HandleManager.MediaPlayerHandles[this], handle);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                AudioProperties.Dispose();
                VideoProperties.Dispose();
                LogProperties.Dispose();
                AudioOutputDevices.Dispose();
                FreeEvents();
                VlcContext.InteropManager.MediaPlayerInterops.ReleaseInstance.Invoke(VlcContext.HandleManager.MediaPlayerHandles[this]);
                VlcContext.HandleManager.MediaPlayerHandles.Remove(this);
            }
        }
    }
}
