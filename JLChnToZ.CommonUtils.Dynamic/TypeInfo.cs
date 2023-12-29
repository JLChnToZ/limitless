using System;
using System.Collections.Generic;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Internal struct that holds type members information.</summary>
    public readonly struct TypeInfo {
        static readonly Dictionary<Type, TypeInfo> cache = new Dictionary<Type, TypeInfo>();
        internal readonly Type type;
        readonly ConstructorInfo[] constructors;
        internal readonly PropertyInfo[] indexers;
        readonly Dictionary<string, MethodInfo[]> methods;
        readonly Dictionary<string, PropertyInfo> properties;
        readonly Dictionary<string, FieldInfo> fields;
        readonly Dictionary<string, EventInfo> events;
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
            events = new Dictionary<string, EventInfo>();
            castInOperators = new Dictionary<Type, MethodInfo>();
            castOutOperators = new Dictionary<Type, MethodInfo>();
            subTypes = new Dictionary<string, Type>();
            var tempMethods = new Dictionary<string, List<MethodInfo>>();
            var tempIndexers = new List<PropertyInfo>();
            var tempConstructors = new List<ConstructorInfo>();
            foreach (var member in type.GetMembers(DEFAULT_FLAGS))
                switch (member.MemberType) {
                    case MemberTypes.Constructor: {
                        tempConstructors.Add(member as ConstructorInfo);
                        break;
                    }
                    case MemberTypes.Event: {
                        var eventInfo = member as EventInfo;
                        events[eventInfo.Name] = eventInfo;
                        break;
                    }
                    case MemberTypes.Field: {
                        var field = member as FieldInfo;
                        fields[field.Name] = field;
                        break;
                    }
                    case MemberTypes.Method: {
                        var method = member as MethodInfo;
                        var methodName = method.Name;
                        switch (methodName) {
                            case "op_Implicit":
                            case "op_Explicit": {
                                var parameters = method.GetParameters();
                                var returnType = method.ReturnType;
                                if (parameters.Length == 1 && returnType != typeof(void)) {
                                    var inputType = parameters[0].ParameterType;
                                    if (returnType == type)
                                        castInOperators[inputType] = method;
                                    else if (inputType == type)
                                        castOutOperators[returnType] = method;
                                }
                                break;
                            }
                            default: {
                                if (method.ContainsGenericParameters) {
                                    int methodCountIndex = methodName.LastIndexOf('`');
                                    if (methodCountIndex >= 0) methodName = methodName.Substring(0, methodCountIndex);
                                }
                                if (!tempMethods.TryGetValue(methodName, out var list))
                                    tempMethods[methodName] = list = new List<MethodInfo>();
                                list.Add(method);
                                break;
                            }
                        }
                        break;
                    }
                    case MemberTypes.Property: {
                        var property = member as PropertyInfo;
                        if (property.GetIndexParameters().Length > 0) {
                            tempIndexers.Add(property);
                            break;
                        }
                        properties[property.Name] = property;
                        break;
                    }
                    case MemberTypes.NestedType: {
                        subTypes[member.Name] = member as Type;
                        break;
                    }
                }
            foreach (var pair in tempMethods) methods[pair.Key] = pair.Value.ToArray();
            indexers = tempIndexers.ToArray();
            constructors = tempConstructors.ToArray();
        }

        internal bool TryGetValue(object instance, string key, out object value) {
            if (properties.TryGetValue(key, out var property) && property.CanRead) {
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
            if (indexes == null || indexers.Length == 0) {
                value = null;
                return false;
            }
            var matched = SelectIndexer(indexers, Array.ConvertAll(indexes, GetUndelyType), null, DEFAULT_FLAGS | BindingFlags.GetProperty);
            if (matched != null) {
                value = InternalWrap(matched.GetValue(instance, InternalUnwrap(indexes, matched.GetIndexParameters())));
                return true;
            }
            value = null;
            return false;
        }

        internal bool TrySetValue(object instance, string key, object value) {
            if (properties.TryGetValue(key, out var property) && property.CanWrite) {
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
            if (indexes == null || indexers.Length == 0) return false;
            var matched = SelectIndexer(indexers, Array.ConvertAll(indexes, GetUndelyType), null, DEFAULT_FLAGS | BindingFlags.SetProperty);
            if (matched != null) {
                matched.SetValue(InternalUnwrap(instance), value, InternalUnwrap(indexes, matched.GetIndexParameters()));
                return true;
            }
            return false;
        }

        internal bool TryGetMethods(string methodName, out MethodInfo[] method) => methods.TryGetValue(methodName, out method);

        internal bool TryGetProperties(string propertyName, out PropertyInfo property) => properties.TryGetValue(propertyName, out property);

        internal bool TryGetConverter(Type type, bool isIn, out MethodInfo method) =>
            (isIn ? castInOperators : castOutOperators).TryGetValue(type, out method);

        internal bool TryInvoke(object instance, string methodName, object[] args, out object result) {
            var safeArgs = args;
            if (methods.TryGetValue(methodName, out var methodArrays) &&
                TryGetMatchingMethod(methodArrays, ref safeArgs, out var resultMethod)) {
                result = InternalWrap(resultMethod.Invoke(resultMethod.IsStatic ? null : InternalUnwrap(instance), safeArgs));
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
            if (type.IsAssignableFrom(isIn ? this.type : instance.GetType())) { // No need to cast if the type is already assignable
                result = instance;
                return true;
            }
            if (TryGetConverter(type, isIn, out var method)) {
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

        internal bool TryGetEvent(string name, out EventInfo evt) => events.TryGetValue(name, out evt);
    }
}
