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
        delegate int DllGETCLASSOBJECTInvoker([MarshalAs(UnmanagedType.LPStruct)]Guid clsid, [MarshalAs(UnmanagedType.LPStruct)]Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        bool _disposed = false;
        Dictionary<string, IntPtr> _libsLoaded = new Dictionary<string, IntPtr>();
        SerializableDictionary<Guid, KnownCOMObject> _knownObjects;

        public string SearchPath
        {
            get;
            private set;
        }

        public URCOMLoader(ITheaterConfigurationManager mbtConfig)
        {
            _knownObjects = mbtConfig.Configuration.InternalPlayerConfiguration.COMConfig.FilterList;
            SearchPath = Path.Combine(mbtConfig.CommonApplicationPaths.ProgramDataPath, "COMObjects");
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

            if (File.Exists(fullDllPath))
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
