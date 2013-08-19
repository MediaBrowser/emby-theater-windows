using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;

/* Change history:
 * 20 Apr 2007  Marc Gravell    Rollback dictionary on error;
 *                              Assert ReflectionPermission for main creation
 *                                  (thanks/credit to Josh Smith for feedback/hints)
 */

namespace MediaBrowser.Theater.Interfaces.Reflection {
    public sealed class HyperTypeDescriptionProvider : TypeDescriptionProvider {
        public static void Add(Type type) {
            TypeDescriptionProvider parent = TypeDescriptor.GetProvider(type);
            TypeDescriptor.AddProvider(new HyperTypeDescriptionProvider(parent), type);
        }
        public HyperTypeDescriptionProvider() : this(typeof(object)) { }
        public HyperTypeDescriptionProvider(Type type) : this(TypeDescriptor.GetProvider(type)) { }
        public HyperTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent) { }
        public static void Clear(Type type) {
            lock (Descriptors) {
                Descriptors.Remove(type);
            }
        }
        public static void Clear() {
            lock (Descriptors) {
                Descriptors.Clear();
            }
        }
        private static readonly Dictionary<Type, ICustomTypeDescriptor> Descriptors = new Dictionary<Type, ICustomTypeDescriptor>();
        public sealed override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
            lock (Descriptors) {
                ICustomTypeDescriptor descriptor;
                if (!Descriptors.TryGetValue(objectType, out descriptor)) {
                    try
                    {
                        descriptor = BuildDescriptor(objectType);
                    }
                    catch
                    {
                        return base.GetTypeDescriptor(objectType, instance);
                    }
                }
                return descriptor;
            }
        }

        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private ICustomTypeDescriptor BuildDescriptor(Type objectType)
        {
            // NOTE: "descriptors" already locked here

            // get the parent descriptor and add to the dictionary so that
            // building the new descriptor will use the base rather than recursing
            ICustomTypeDescriptor descriptor = base.GetTypeDescriptor(objectType, null);
            Descriptors.Add(objectType, descriptor);
            try
            {
                // build a new descriptor from this, and replace the lookup
                descriptor = new HyperTypeDescriptor(descriptor);
                Descriptors[objectType] = descriptor;
                return descriptor;
            }
            catch
            {   // rollback and throw
                // (perhaps because the specific caller lacked permissions;
                // another caller may be successful)
                Descriptors.Remove(objectType);
                throw;
            }
        }
    }
}
