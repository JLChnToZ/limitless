using System;
using System.Collections.Generic;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Internal struct that holds type members information.</summary>
    public readonly struct TypeInfo {
        static readonly Dictionary<Type, TypeInfo> cache = new Dictionary<Type, TypeInfo>();
        readonly Type type;
        readonly ConstructorInfo[] constructors;
        readonly PropertyInfo[] indexers;
        readonly Dictionary<string, MethodInfo[]> methods;
        readonly Dictionary<string, PropertyInfo> properties;
        readonly Dictionary<string, FieldInfo> fields;
        readonly Dictionary<Type, MethodInfo> castInOperators;
        readonly Dictionary<Type, MethodInfo> castOutOperators;
        readonly Dictionary<string, Type> subTypes;

        public static TypeInfo Get(Type type) {
            if (!cache.TryGetValue(type, out var typeInfo))
                cache[type] = typeInfo = new TypeInfo(type);
            return typeInfo;
        }

        TypeInfo(Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            this.type = type;
            methods = new Dictionary<string, MethodInfo[]>();
            properties = new Dictionary<string, PropertyInfo>();
            fields = new Dictionary<string, FieldInfo>();
            castInOperators = new Dictionary<Type, MethodInfo>();
            castOutOperators = new Dictionary<Type, MethodInfo>();
            subTypes = new Dictionary<string, Type>();
            var tempMethods = new Dictionary<string, List<MethodInfo>>();
            foreach (var m in type.GetMethods(DEFAULT_FLAGS)) {
                var methodName = m.Name;
                if (m.ContainsGenericParameters) {
                    int methodCountIndex = methodName.LastIndexOf('`');
                    if (methodCountIndex >= 0) methodName = methodName.Substring(0, methodCountIndex);
                }
                if (!tempMethods.TryGetValue(methodName, out var list))
                    tempMethods[methodName] = list = new List<MethodInfo>();
                list.Add(m);
                switch (methodName) {
                    case "op_Implicit":
                    case "op_Explicit": {
                        var parameters = m.GetParameters();
                        var returnType = m.ReturnType;
                        if (parameters.Length == 1 && returnType != typeof(void)) {
                            var inputType = parameters[0].ParameterType;
                            if (returnType == type)
                                castInOperators[inputType] = m;
                            else if (inputType == type)
                                castOutOperators[returnType] = m;
                        }
                        break;
                    }
                }
            }
            foreach (var kv in tempMethods) methods[kv.Key] = kv.Value.ToArray();
            var tempIndexers = new List<PropertyInfo>();
            foreach (var p in type.GetProperties(DEFAULT_FLAGS)) {
                properties[p.Name] = p;
                if (p.GetIndexParameters().Length > 0) tempIndexers.Add(p);
            }
            indexers = tempIndexers.ToArray();
            foreach (var f in type.GetFields(DEFAULT_FLAGS)) fields[f.Name] = f;
            constructors = type.GetConstructors(INSTANCE_FLAGS);
            foreach (var t in type.GetNestedTypes(DEFAULT_FLAGS)) subTypes[t.Name] = t;
        }

        internal bool TryGetValue(object instance, string key, out object value) {
            if (properties.TryGetValue(key, out var property) && property.CanRead && property.GetIndexParameters().Length == 0) {
                value = InternalWrap(property.GetValue(instance));
                return true;
            }
            if (fields.TryGetValue(key, out var field)) {
                value = InternalWrap(field.GetValue(instance));
                return true;
            }
            value = null;
            return false;
        }

        internal bool TryGetValue(object instance, object[] indexes, out object value) {
            PropertyInfo matched;
            if (indexes == null || indexers.Length == 0) {
                value = null;
                return false;
            }
            matched = Type.DefaultBinder.SelectProperty(
                DEFAULT_FLAGS | BindingFlags.GetProperty, indexers, null, Array.ConvertAll(indexes, GetUndelyType), null
            );
            if (matched != null) {
                value = InternalWrap(matched.GetValue(instance, InternalUnwrap(
                    indexes, Type.DefaultBinder,
                    Array.ConvertAll(matched.GetIndexParameters(), GetParameterType)
                )));
                return true;
            }
            value = null;
            return false;
        }

        internal bool TrySetValue(object instance, string key, object value) {
            if (properties.TryGetValue(key, out var property) && property.CanWrite && property.GetIndexParameters().Length == 0) {
                property.SetValue(InternalUnwrap(instance), value);
                return true;
            }
            if (fields.TryGetValue(key, out var field) && !field.IsInitOnly) {
                field.SetValue(InternalUnwrap(instance), value);
                return true;
            }
            return false;
        }

        internal bool TrySetValue(object instance, object[] indexes, object value) {
            PropertyInfo matched;
            if (indexes == null || indexers.Length == 0) return false;
            matched = Type.DefaultBinder.SelectProperty(
                DEFAULT_FLAGS | BindingFlags.SetProperty, indexers, null, Array.ConvertAll(indexes, GetUndelyType), null
            );
            if (matched != null) {
                matched.SetValue(InternalUnwrap(instance), value, InternalUnwrap(
                    indexes, Type.DefaultBinder,
                    Array.ConvertAll(matched.GetIndexParameters(), GetParameterType)
                ));
                return true;
            }
            return false;
        }

        internal bool TryGetMethods(string methodName, out MethodInfo[] method) => methods.TryGetValue(methodName, out method);

        internal bool TryInvoke(object instance, string methodName, object[] args, out object result) {
            var safeArgs = args;
            if (methods.TryGetValue(methodName, out var methodArrays) &&
                TryGetMatchingMethod(methodArrays, ref safeArgs, out var resultMethod)) {
                result = InternalWrap(resultMethod.Invoke(InternalUnwrap(instance), safeArgs));
                InternalWrap(safeArgs, args);
                return true;
            }
            result = null;
            return false;
        }

        internal bool TryCast(object instance, Type type, bool isIn, out object result) {
            // Note: Return results after casting must not be wrapped.
            instance = InternalUnwrap(instance);
            if (instance == null) {
                result = null;
                return !(isIn ? this.type : type).IsValueType; // Only reference types can be null
            }
            var instanceType = isIn ? this.type : instance.GetType();
            if (instanceType.IsAssignableFrom(type) || type.IsAssignableFrom(instanceType)) { // No need to cast if the type is already assignable
                result = instance;
                return true;
            }
            if ((isIn ? castInOperators : castOutOperators).TryGetValue(type, out var method)) {
                result = method.Invoke(null, new[] { instance });
                return true;
            }
            result = null;
            return false;
        }

        internal bool TryConstruct(object[] args, out object result) {
            if (TryGetMatchingMethod(constructors, ref args, out var resultConstructor)) {
                result = InternalWrap(resultConstructor.Invoke(args));
                return true;
            }
            result = null;
            return false;
        }

        internal bool TryGetSubType(string name, out Type type) => subTypes.TryGetValue(name, out type);
    }
}
