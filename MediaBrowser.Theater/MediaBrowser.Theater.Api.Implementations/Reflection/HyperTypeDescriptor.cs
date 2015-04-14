/* Change history:
 * 20 Apr 2007  Marc Gravell    Renamed
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace MediaBrowser.Theater.Api.Reflection
{
    internal sealed class HyperTypeDescriptor : CustomTypeDescriptor
    {
        private static readonly Dictionary<PropertyInfo, PropertyDescriptor> Properties = new Dictionary<PropertyInfo, PropertyDescriptor>();
        private static readonly ModuleBuilder ModuleBuilder;
        private static int _counter;
        private readonly PropertyDescriptorCollection _propertyCollections;

        static HyperTypeDescriptor()
        {
            var an = new AssemblyName("Hyper.ComponentModel.dynamic");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder = ab.DefineDynamicModule("Hyper.ComponentModel.dynamic.dll");
        }

        internal HyperTypeDescriptor(ICustomTypeDescriptor parent)
            : base(parent)
        {
            _propertyCollections = WrapProperties(parent.GetProperties());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return _propertyCollections;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return _propertyCollections;
        }

        private static PropertyDescriptorCollection WrapProperties(PropertyDescriptorCollection oldProps)
        {
            var newProps = new PropertyDescriptor[oldProps.Count];
            int index = 0;
            bool changed = false;
            // HACK: how to identify reflection, given that the class is internal...
            Type wrapMe = Assembly.GetAssembly(typeof (PropertyDescriptor)).GetType("System.ComponentModel.ReflectPropertyDescriptor");
            foreach (PropertyDescriptor oldProp in oldProps) {
                PropertyDescriptor pd = oldProp;
                // if it looks like reflection, try to create a bespoke descriptor
                if (ReferenceEquals(wrapMe, pd.GetType()) && TryCreatePropertyDescriptor(ref pd)) {
                    changed = true;
                }
                newProps[index++] = pd;
            }

            return changed ? new PropertyDescriptorCollection(newProps, true) : oldProps;
        }

        private static bool TryCreatePropertyDescriptor(ref PropertyDescriptor descriptor)
        {
            try {
                PropertyInfo property = descriptor.ComponentType.GetProperty(descriptor.Name);
                if (property == null) {
                    return false;
                }

                lock (Properties) {
                    PropertyDescriptor foundBuiltAlready;
                    if (Properties.TryGetValue(property, out foundBuiltAlready)) {
                        descriptor = foundBuiltAlready;
                        return true;
                    }

                    string name = "_c" + Interlocked.Increment(ref _counter);
                    TypeBuilder tb = ModuleBuilder.DefineType(name, TypeAttributes.Sealed | TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.Public, typeof (ChainingPropertyDescriptor));

                    // ctor calls base
                    ConstructorBuilder cb = tb.DefineConstructor(MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { typeof (PropertyDescriptor) });
                    ILGenerator il = cb.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, typeof (ChainingPropertyDescriptor).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof (PropertyDescriptor) }, null));
                    il.Emit(OpCodes.Ret);

                    MethodBuilder mb;
                    MethodInfo baseMethod;
                    if (property.CanRead) {
                        // obtain the implementation that we want to override
                        baseMethod = typeof (ChainingPropertyDescriptor).GetMethod("GetValue");
                        // create a new method that accepts an object and returns an object (as per the base)
                        mb = tb.DefineMethod(baseMethod.Name,
                                             MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final,
                                             baseMethod.CallingConvention, baseMethod.ReturnType, new[] { typeof (object) });
                        // start writing IL into the method
                        il = mb.GetILGenerator();
                        if (property.DeclaringType.IsValueType) {
                            // upbox the object argument into our known (instance) struct type
                            LocalBuilder lb = il.DeclareLocal(property.DeclaringType);
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Unbox_Any, property.DeclaringType);
                            il.Emit(OpCodes.Stloc_0);
                            il.Emit(OpCodes.Ldloca_S, lb);
                        } else {
                            // cast the object argument into our known class type
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Castclass, property.DeclaringType);
                        }
                        // call the "get" method
                        il.Emit(OpCodes.Callvirt, property.GetGetMethod());

                        if (property.PropertyType.IsValueType) {
                            // box it from the known (value) struct type
                            il.Emit(OpCodes.Box, property.PropertyType);
                        }
                        // return the value
                        il.Emit(OpCodes.Ret);
                        // signal that this method should override the base
                        tb.DefineMethodOverride(mb, baseMethod);
                    }

                    bool supportsChangeEvents = descriptor.SupportsChangeEvents, isReadOnly = descriptor.IsReadOnly;

                    // override SupportsChangeEvents
                    baseMethod = typeof (ChainingPropertyDescriptor).GetProperty("SupportsChangeEvents").GetGetMethod();
                    mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName, baseMethod.CallingConvention, baseMethod.ReturnType, Type.EmptyTypes);
                    il = mb.GetILGenerator();
                    if (supportsChangeEvents) {
                        il.Emit(OpCodes.Ldc_I4_1);
                    } else {
                        il.Emit(OpCodes.Ldc_I4_0);
                    }
                    il.Emit(OpCodes.Ret);
                    tb.DefineMethodOverride(mb, baseMethod);

                    // override IsReadOnly
                    baseMethod = typeof (ChainingPropertyDescriptor).GetProperty("IsReadOnly").GetGetMethod();
                    mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName, baseMethod.CallingConvention, baseMethod.ReturnType, Type.EmptyTypes);
                    il = mb.GetILGenerator();
                    if (isReadOnly) {
                        il.Emit(OpCodes.Ldc_I4_1);
                    } else {
                        il.Emit(OpCodes.Ldc_I4_0);
                    }
                    il.Emit(OpCodes.Ret);
                    tb.DefineMethodOverride(mb, baseMethod);

                    /*  REMOVED: PropertyType, ComponentType; actually *adds* time overriding these
                    // override PropertyType
                    baseMethod = typeof(ChainingPropertyDescriptor).GetProperty("PropertyType").GetGetMethod();
                    mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName, baseMethod.CallingConvention, baseMethod.ReturnType, Type.EmptyTypes);
                    il = mb.GetILGenerator();
                    il.Emit(OpCodes.Ldtoken, descriptor.PropertyType);
                    il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                    il.Emit(OpCodes.Ret);
                    tb.DefineMethodOverride(mb, baseMethod);

                    // override ComponentType
                    baseMethod = typeof(ChainingPropertyDescriptor).GetProperty("ComponentType").GetGetMethod();
                    mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName, baseMethod.CallingConvention, baseMethod.ReturnType, Type.EmptyTypes);
                    il = mb.GetILGenerator();
                    il.Emit(OpCodes.Ldtoken, descriptor.ComponentType);
                    il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                    il.Emit(OpCodes.Ret);
                    tb.DefineMethodOverride(mb, baseMethod);
                    */

                    // for classes, implement write (would be lost in unbox for structs)
                    if (!property.DeclaringType.IsValueType) {
                        if (!isReadOnly && property.CanWrite) {
                            // override set method
                            baseMethod = typeof (ChainingPropertyDescriptor).GetMethod("SetValue");
                            mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, baseMethod.CallingConvention, baseMethod.ReturnType, new[] { typeof (object), typeof (object) });
                            il = mb.GetILGenerator();
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Castclass, property.DeclaringType);
                            il.Emit(OpCodes.Ldarg_2);
                            if (property.PropertyType.IsValueType) {
                                il.Emit(OpCodes.Unbox_Any, property.PropertyType);
                            } else {
                                il.Emit(OpCodes.Castclass, property.PropertyType);
                            }
                            il.Emit(OpCodes.Callvirt, property.GetSetMethod());
                            il.Emit(OpCodes.Ret);
                            tb.DefineMethodOverride(mb, baseMethod);
                        }

                        if (supportsChangeEvents) {
                            EventInfo ei = property.DeclaringType.GetEvent(property.Name + "Changed");
                            if (ei != null) {
                                baseMethod = typeof (ChainingPropertyDescriptor).GetMethod("AddValueChanged");
                                mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName, baseMethod.CallingConvention, baseMethod.ReturnType, new[] { typeof (object), typeof (EventHandler) });
                                il = mb.GetILGenerator();
                                il.Emit(OpCodes.Ldarg_1);
                                il.Emit(OpCodes.Castclass, property.DeclaringType);
                                il.Emit(OpCodes.Ldarg_2);
                                il.Emit(OpCodes.Callvirt, ei.GetAddMethod());
                                il.Emit(OpCodes.Ret);
                                tb.DefineMethodOverride(mb, baseMethod);

                                baseMethod = typeof (ChainingPropertyDescriptor).GetMethod("RemoveValueChanged");
                                mb = tb.DefineMethod(baseMethod.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName, baseMethod.CallingConvention, baseMethod.ReturnType, new[] { typeof (object), typeof (EventHandler) });
                                il = mb.GetILGenerator();
                                il.Emit(OpCodes.Ldarg_1);
                                il.Emit(OpCodes.Castclass, property.DeclaringType);
                                il.Emit(OpCodes.Ldarg_2);
                                il.Emit(OpCodes.Callvirt, ei.GetRemoveMethod());
                                il.Emit(OpCodes.Ret);
                                tb.DefineMethodOverride(mb, baseMethod);
                            }
                        }
                    }
                    var newDesc = tb.CreateType().GetConstructor(new[] { typeof (PropertyDescriptor) }).Invoke(new object[] { descriptor }) as PropertyDescriptor;
                    if (newDesc == null) {
                        return false;
                    }
                    descriptor = newDesc;
                    Properties.Add(property, descriptor);
                    return true;
                }
            } catch {
                return false;
            }
        }
    }
}