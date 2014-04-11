using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using DirectShowLib;
using System.Text.RegularExpressions;
using DirectShowLib.Utils;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces;
using System.Reflection;
using System.Net;
using System.Threading;
using MediaBrowser.Common.Implementations.Archiving;

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

        public static void EnsureObjects(ITheaterConfigurationManager mbtConfig, bool block)
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

                DateTime lastCheckDate = ReadTextDate(lastCheckedPath);
                if (lastCheckDate.AddDays(7) > DateTime.Now)
                    needsCheck = false;

                if (needsCheck)
                {
                    if (block)
                        CheckObjects(objPath);
                    else
                        ThreadPool.QueueUserWorkItem(new WaitCallback(CheckObjects), objPath);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static DateTime ReadTextDate(string filePath)
        {
            DateTime textDate = DateTime.MinValue;

            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string firstLine = sr.ReadLine();
                        if (!DateTime.TryParse(firstLine, out textDate))
                        {
                            textDate = DateTime.MinValue; //should be unecessary, but...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return textDate;
        }

        private static void WriteTextDate(string filePath, DateTime theDate)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine(theDate);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void CheckObjects(object objDlPath)
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
                        ZipClient zc = new ZipClient();
                        string[] objToCheck = dlList.Split(new string[]{System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string toCheck in objToCheck)
                        {
                            string txtPath = Path.Combine(dlPath, Path.ChangeExtension(toCheck, "txt"));
                            DateTime lastUpdateDate = ReadTextDate(txtPath);
                            Uri comPath = new Uri(Path.Combine(Path.Combine(System.Configuration.ConfigurationSettings.AppSettings["PrivateObjectsManifest"], toCheck)));
                            WebRequest request = WebRequest.Create(comPath);
                            request.Method = "HEAD";
                            using(WebResponse wr = request.GetResponse())
                            {                                
                                DateTime lmDate;
                                if (DateTime.TryParse(wr.Headers[HttpResponseHeader.LastModified], out lmDate))
                                {
                                    if (lmDate > lastUpdateDate)
                                    {
                                        //download the updated component
                                        byte[] comBin = mwc.DownloadData(comPath);
                                        if (comBin.Length > 0)
                                        {
                                            using (MemoryStream ms = new MemoryStream(comBin))
                                            {
                                                zc.ExtractAll(ms, dlPath, true);
                                            }
                                        }
                                        WriteTextDate(txtPath, lmDate);
                                    }
                                }
                            }
                        }
                    }
                }

                WriteTextDate(lastCheckedPath, DateTime.Now);
            }
            catch (Exception ex)
            {
            }
        }

        public string SearchPath
        {
            get;
            private set;
        }

        public URCOMLoader(ITheaterConfigurationManager mbtConfig)
        {
            //this should be called on app load, but this will make sure it gets done.
            URCOMLoader.EnsureObjects(mbtConfig, true);

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
}
