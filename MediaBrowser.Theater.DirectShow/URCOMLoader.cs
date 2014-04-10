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

        private static string _searchPath = string.Empty;
        public static string SearchPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_searchPath))
                {
                    _searchPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "COMObjects");
                }
                return _searchPath;
            }
        }

        public URCOMLoader(KnownCOMObjectConfiguration kfs)
        {
            _knownObjects = kfs.FilterList;
        }

        public object CreateObjectFromPath(string dllPath, Guid clsid)
        {
            return CreateObjectFromPath(dllPath, clsid, false);
        }

        //http://www.gdcl.co.uk/2011/June/UnregisteredFilters.htm
        public object CreateObjectFromPath(string dllPath, Guid clsid, bool setSearchPath)
        {
            object createdObject = null;
            IntPtr lib = IntPtr.Zero;

            if (_libsLoaded.ContainsKey(dllPath))
                lib = _libsLoaded[dllPath];
            else
            {
                //some dlls have external dependancies, setting the search path to its location should assist with this
                if (setSearchPath)
                {
                    NativeMethods.SetDllDirectory(URCOMLoader.SearchPath);
                }

                lib = NativeMethods.LoadLibrary(dllPath);

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
            else
            {
                throw new Win32Exception();
            }

            return createdObject;
        }

        public IBaseFilter LoadFilter(Guid guid, IGraphBuilder graph)
        {
            if (_knownObjects.ContainsKey(guid))
                return LoadFilter(_knownObjects[guid], graph);
            else
                return null;
        }

        public IBaseFilter LoadFilter(string filterName, IGraphBuilder graph)
        {
            foreach (KnownCOMObject kf in _knownObjects.Values)
            {
                if (string.Compare(kf.ObjectName, filterName, true) == 0)
                    return LoadFilter(kf, graph);
            }
            return null;
        }

        public object GetObject(Guid guid)
        {
            if (_knownObjects.ContainsKey(guid))
                return GetObject(_knownObjects[guid]);
            else
                return null;
        }

        public object GetObject(string filterName)
        {
            foreach (KnownCOMObject kf in _knownObjects.Values)
            {
                if (string.Compare(kf.ObjectName, filterName, true) == 0)
                    return GetObject(kf);
            }
            return null;
        }

        public object GetObject(KnownCOMObject kf)
        {
            try
            {
                return this.CreateObjectFromPath(kf.ObjectPath, kf.Clsid, true);
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

        public IBaseFilter LoadFilter(KnownCOMObject kf, IGraphBuilder graph)
        {
            try
            {
                IBaseFilter filter = this.CreateObjectFromPath(kf.ObjectPath, kf.Clsid, true) as IBaseFilter;
                if (filter != null && graph != null)
                {
                    int hr = graph.AddFilter(filter, kf.ObjectName);
                    DsError.ThrowExceptionForHR(hr);
                }
                return filter;
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

        public List<IBaseFilter> AddPreferredFilters(IGraphBuilder graph, string PreferredFilters)
        {
            List<IBaseFilter> pFilters = new List<IBaseFilter>();

            foreach (string filtName in PreferredFilters.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                IBaseFilter filter = null;
                try
                {
                    Guid filtGuid;

                    if (KnownCOMObjectConfiguration.IsGuid(filtName, out filtGuid))
                    {
                        filter = this.LoadFilter(filtGuid, graph);
                        
                        if(filter == null)
                            filter = FilterGraphTools.AddFilterFromClsid(graph, filtGuid, filtName);
                    }
                    else
                    {
                        filter = this.LoadFilter(filtName, graph);

                        if (filter == null)
                            filter = FilterGraphTools.AddFilterByName(graph, FilterCategory.LegacyAmFilterCategory, filtName);
                    }

                    if (filter != null)
                    {
                        Console.WriteLine("Added {0} to the graph", filtName);
                        pFilters.Add(filter);
                    }
                    else
                        Console.WriteLine("{0} not added to the graph", filtName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error {1} adding {0} to the graph", filtName, ex.Message);
                }
            }
            return pFilters;
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
