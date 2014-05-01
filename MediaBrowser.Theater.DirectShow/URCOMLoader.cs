using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using DirectShowLib;
using System.Text.RegularExpressions;
using DirectShowLib.Utils;
using MediaBrowser.Model.IO;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces;
using System.Reflection;
using System.Net;
using System.Threading;

namespace MediaBrowser.Theater.DirectShow
{
    [ComVisible(true), ComImport(),
    Guid("00000001-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int CreateInstance(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object obj);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int LockServer(
            [MarshalAs(UnmanagedType.Bool), In] bool fLock);
    }

    public class URCOMLoader : IDisposable
    {
        const string OJB_FOLDER = "COMObjects";
        const string LAST_CHECKED = "LastChecked.txt";
        delegate int DllGETCLASSOBJECTInvoker([MarshalAs(UnmanagedType.LPStruct)]Guid clsid, [MarshalAs(UnmanagedType.LPStruct)]Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        bool _disposed = false;
        Dictionary<string, IntPtr> _libsLoaded = new Dictionary<string, IntPtr>();
        SerializableDictionary<Guid, KnownCOMObject> _knownObjects;
        bool _preferURObjects = true;

        private static string _exeVersion = string.Empty;
        public static string ExeVersion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_exeVersion))
                {
                    _exeVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                }
                return _exeVersion;
            }
        }

        public static void EnsureObjects(ITheaterConfigurationManager mbtConfig, bool block, IZipClient zipClient)
        {
            try
            {
                string objPath = Path.Combine(mbtConfig.CommonApplicationPaths.ProgramDataPath, OJB_FOLDER);
                string lastCheckedPath = Path.Combine(objPath, LAST_CHECKED);
                bool needsCheck = true;

                if (!Directory.Exists(objPath))
                {
                    Directory.CreateDirectory(objPath);
                }

                DateAndVersion lastCheck = new DateAndVersion(lastCheckedPath);
                if (lastCheck.StoredDate.AddDays(7) > DateTime.Now)
                    needsCheck = false;
                if (lastCheck.VersionNumber != ExeVersion)
                    needsCheck = true;

                if (needsCheck)
                {
                    if (block)
                        CheckObjects(objPath, zipClient);
                    else
                        ThreadPool.QueueUserWorkItem(o => CheckObjects(o, zipClient), objPath);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void CheckObjects(object objDlPath, IZipClient zipClient)
        {
            try
            {
                Uri objManifest = new Uri(Path.Combine(System.Configuration.ConfigurationSettings.AppSettings["PrivateObjectsManifest"], "manifest.txt"));
                string dlPath = objDlPath.ToString();
                string lastCheckedPath = Path.Combine(dlPath, LAST_CHECKED);

                using (WebClient mwc = new WebClient())
                {
                    string dlList = mwc.DownloadString(objManifest);
                    if (!string.IsNullOrWhiteSpace(dlList))
                    {
                        string[] objToCheck = dlList.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string toCheck in objToCheck)
                        {
                            string txtPath = Path.Combine(dlPath, Path.ChangeExtension(toCheck, "txt"));
                            DateAndVersion lastUpdate = new DateAndVersion(txtPath);
                            Uri comPath = new Uri(Path.Combine(Path.Combine(System.Configuration.ConfigurationSettings.AppSettings["PrivateObjectsManifest"], toCheck)));
                            WebRequest request = WebRequest.Create(comPath);
                            request.Method = "HEAD";

                            using (WebResponse wr = request.GetResponse())
                            {
                                DateTime lmDate;
                                if (DateTime.TryParse(wr.Headers[HttpResponseHeader.LastModified], out lmDate))
                                {
                                    if (lmDate > lastUpdate.StoredDate)
                                    {
                                        //download the updated component
                                        using (WebClient fd = new WebClient())
                                        {
                                            fd.DownloadProgressChanged += fd_DownloadProgressChanged;
                                            byte[] comBin = fd.DownloadData(comPath);
                                            if (comBin.Length > 0)
                                            {
                                                using (MemoryStream ms = new MemoryStream(comBin))
                                                {
                                                    zipClient.ExtractAll(ms, dlPath, true);
                                                }

                                                DateAndVersion.Write(new DateAndVersion(txtPath, lmDate, ExeVersion));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                DateAndVersion.Write(new DateAndVersion(lastCheckedPath, DateTime.Now, ExeVersion));
            }
            catch (Exception ex)
            {
            }
        }

        static void fd_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //pump status upward
        }

        public string SearchPath
        {
            get;
            private set;
        }

        public URCOMLoader(ITheaterConfigurationManager mbtConfig, IZipClient zipClient)
        {
            //this should be called on app load, but this will make sure it gets done.
            URCOMLoader.EnsureObjects(mbtConfig, true, zipClient);

            _knownObjects = mbtConfig.Configuration.InternalPlayerConfiguration.COMConfig.FilterList;
            SearchPath = Path.Combine(mbtConfig.CommonApplicationPaths.ProgramDataPath, OJB_FOLDER);
            _preferURObjects = mbtConfig.Configuration.InternalPlayerConfiguration.UsePrivateObjects;
        }

        public object CreateObjectFromPath(string dllPath, Guid clsid, bool comFallback)
        {
            return CreateObjectFromPath(dllPath, clsid, false, comFallback);
        }

        //http://www.gdcl.co.uk/2011/June/UnregisteredFilters.htm
        public object CreateObjectFromPath(string dllPath, Guid clsid, bool setSearchPath, bool comFallback)
        {
            object createdObject = null;
            IntPtr lib = IntPtr.Zero;
            string fullDllPath = Path.Combine(SearchPath, dllPath);

            if (File.Exists(fullDllPath) && (_preferURObjects || !comFallback))
            {
                if (_libsLoaded.ContainsKey(dllPath))
                    lib = _libsLoaded[dllPath];
                else
                {
                    //some dlls have external dependancies, setting the search path to its location should assist with this
                    if (setSearchPath)
                    {
                        NativeMethods.SetDllDirectory(Path.GetDirectoryName(fullDllPath));
                    }

                    lib = NativeMethods.LoadLibrary(fullDllPath);

                    if (setSearchPath)
                    {
                        NativeMethods.SetDllDirectory(null);
                    }
                }

                if (lib != IntPtr.Zero)
                {
                    //we need to cache the handle so the COM object will work and we can clean up later
                    _libsLoaded[dllPath] = lib;
                    IntPtr fnP = NativeMethods.GetProcAddress(lib, "DllGetClassObject");
                    if (fnP != IntPtr.Zero)
                    {
                        DllGETCLASSOBJECTInvoker fn = Marshal.GetDelegateForFunctionPointer(fnP, typeof(DllGETCLASSOBJECTInvoker)) as DllGETCLASSOBJECTInvoker;

                        object pUnk = null;
                        int hr = fn(clsid, IID_IUnknown, out pUnk);
                        if (hr >= 0)
                        {
                            IClassFactory pCF = pUnk as IClassFactory;
                            if (pCF != null)
                            {
                                hr = pCF.CreateInstance(null, IID_IUnknown, out createdObject);
                            }
                        }
                    }
                    else
                    {
                        throw new Win32Exception();
                    }
                }
                else if (comFallback)
                {
                    Type type = Type.GetTypeFromCLSID(clsid);
                    return Activator.CreateInstance(type);
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            else if (comFallback)
            {
                Type type = Type.GetTypeFromCLSID(clsid);
                return Activator.CreateInstance(type);
            }

            return createdObject;
        }

        public object GetObject(Guid guid, bool comFallback)
        {
            if (_knownObjects.ContainsKey(guid))
                return GetObject(_knownObjects[guid], comFallback);
            else
                return null;
        }

        public object GetObject(string filterName, bool comFallback)
        {
            foreach (KnownCOMObject kf in _knownObjects.Values)
            {
                if (string.Compare(kf.ObjectName, filterName, true) == 0)
                    return GetObject(kf, comFallback);
            }
            return null;
        }

        public object GetObject(KnownCOMObject kf, bool comFallback)
        {
            try
            {
                return this.CreateObjectFromPath(kf.ObjectPath, kf.Clsid, true, comFallback);
            }
            catch (COMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not load {0} - {1}: {2}", kf.Clsid, kf.ObjectPath, ex.Message);
                return null;
            }
        }

        #region IDisposable Members

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }

                foreach (string key in _libsLoaded.Keys)
                {
                    IntPtr lib = _libsLoaded[key];
                    NativeMethods.FreeLibrary(lib);
                }

                _libsLoaded = new Dictionary<string, IntPtr>();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public class DateAndVersion
    {
        public string FilePath { get; private set; }

        public DateTime StoredDate { get; set; }
        public string VersionNumber { get; set; }

        public DateAndVersion(string filePath)
        {
            FilePath = filePath;
            StoredDate = DateTime.MinValue;
            VersionNumber = string.Empty;

            try
            {
                DateTime textDate;
                if (File.Exists(FilePath))
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string firstLine = sr.ReadLine();
                        if (DateTime.TryParse(firstLine, out textDate))
                        {
                            StoredDate = textDate;
                        }
                        string secondLine = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(secondLine))
                            VersionNumber = secondLine;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public DateAndVersion(string filePath, DateTime storedDate, string versionNumber)
        {
            FilePath = filePath;
            StoredDate = storedDate;
            VersionNumber = versionNumber;
        }

        public void Write()
        {
            DateAndVersion.Write(this);
        }

        public static void Write(DateAndVersion dv)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(dv.FilePath))
                {
                    sw.WriteLine(dv.StoredDate);
                    sw.WriteLine(dv.VersionNumber);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}