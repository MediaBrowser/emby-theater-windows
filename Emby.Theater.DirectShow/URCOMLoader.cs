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
//using MediaBrowser.Theater.Interfaces.Configuration;
//using MediaBrowser.Theater.Interfaces;
using System.Reflection;
using System.Net;
using System.Threading;
using System.Diagnostics;
using MediaBrowser.Model.Logging;
using Emby.Theater.DirectShow.Configuration;
using MediaBrowser.Common.Configuration;

namespace Emby.Theater.DirectShow
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
        delegate int DllGETCLASSOBJECTInvoker([MarshalAs(UnmanagedType.LPStruct)]Guid clsid, [MarshalAs(UnmanagedType.LPStruct)]Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        bool _disposed = false;
        Dictionary<string, IntPtr> _libsLoaded = new Dictionary<string, IntPtr>();
        SerializableDictionary<Guid, KnownCOMObject> _knownObjects;
        bool _preferURObjects = true;
        bool _initialized = false;
        ILogger _logger = null;
        static object _instanceLock = new object();
        ManualResetEvent _mreFilterBlock = new ManualResetEvent(true);
        private IConfigurationManager _configurationManager;

        private static URCOMLoader _instance = null;
        public static URCOMLoader Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                        _instance = new URCOMLoader();
                    return _instance;
                }
            }
        }

        private void ExtractBytes(string filename, string dlPath, IZipClient zipClient)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".FilterZips." + filename))
            {
                _logger.Info("ExtractBytes: {0}", dlPath);
                zipClient.ExtractAll(stream, dlPath, true);
            }
        }

        private string GetComObjectsFilterPath(string appProgramDataPath)
        {
            return Path.Combine(appProgramDataPath, OJB_FOLDER, "2017-06-12");
        }

        public bool EnsureObjects(string appProgramDataPath, IZipClient zipClient)
        {
            bool needsRestart = false;

            try
            {
                string objPath = GetComObjectsFilterPath(appProgramDataPath);

                if (!Directory.Exists(objPath) || Directory.GetDirectories(objPath).Length == 0)
                {
                    Directory.CreateDirectory(objPath);
                    //extract embedded filters

                    ExtractBytes("LAV.zip", objPath, zipClient);
                    ExtractBytes("madVR.zip", objPath, zipClient);
                    ExtractBytes("mpaudio.zip", objPath, zipClient);
                    ExtractBytes("XySubFilter.zip", objPath, zipClient);
                    ExtractBytes("xy-VSFilter.zip", objPath, zipClient);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("EnsureObjects Error: {0}", ex.Message);
            }

            _logger.Debug("EnsureObjects needsRestart: {0}", needsRestart);
            return needsRestart;
        }

        public string SearchPath
        {
            get;
            private set;
        }

        private URCOMLoader()
        {
            //init code moved to Initialize which must be called before this object will work correctly
        }

        private KnownCOMObjectConfiguration _KnownCOMObjectConfiguration = new KnownCOMObjectConfiguration();

        public string Initialize(string appProgramDataPath, IZipClient zipClient, ILogManager logManager, IConfigurationManager configurationManager)
        {
            if (!_initialized)
            {
                _configurationManager = configurationManager;

                _KnownCOMObjectConfiguration.SetDefaults();

                _knownObjects = _KnownCOMObjectConfiguration.FilterList;
                SearchPath = GetComObjectsFilterPath(appProgramDataPath);

                _logger = logManager.GetLogger("URCOMLoader"); 

                _logger.Debug("URCOMLoader Initialized");

                _initialized = true;
            }

            return SearchPath;
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

            _logger.Info("CreateObjectFromPath: {0} - {1} - {2} - {3}", fullDllPath, clsid, setSearchPath, comFallback);

            if (File.Exists(fullDllPath) && (_preferURObjects || !comFallback))
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(fullDllPath);
                _logger.Debug("Load: {0} Version: {1}", fileVersionInfo.FileDescription, fileVersionInfo.FileVersion);
                if (_libsLoaded.ContainsKey(dllPath))
                {
                    _logger.Debug("Load: {0} from cache", dllPath);
                    lib = _libsLoaded[dllPath];
                }
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
                                _logger.Debug("CreateInstance {0}: {1}", fileVersionInfo.FileDescription, hr);
                            }
                        }
                    }
                    else
                    {
                        _logger.Debug("Couldn't load {0}", fileVersionInfo.FileDescription);
                        throw new Win32Exception();
                    }
                }
                else if (comFallback)
                {
                    _logger.Debug("No lib, load from Global COM {0}", clsid);
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
                _logger.Debug("Load from Global COM {0}", clsid);
                Type type = Type.GetTypeFromCLSID(clsid);
                return Activator.CreateInstance(type);
            }

            _logger.Debug("Got Object {0} from {1}", createdObject, clsid);
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
                //TODO: might be better to call _mreFilterBlock.WaitOne with a small value (e.g. 1000) and surface an actionalbe result if it fails so the UI can signal a potentially long running process
                if (_mreFilterBlock.WaitOne(_KnownCOMObjectConfiguration.LoadWait))
                {
                    _logger.Debug("URCOMLoader is not blocking.");
                    return this.CreateObjectFromPath(kf.ObjectPath, kf.Clsid, true, comFallback);
                }
                else
                {
                    _logger.Debug("URCOMLoader is blocking, failed to load object");
                    return null;
                }
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