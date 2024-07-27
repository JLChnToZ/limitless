using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Internal struct that holds type members information.</summary>
    public readonly struct TypeInfo : IEquatable<TypeInfo> {
        static readonly Dictionary<Type, TypeInfo> cache = new Dictionary<Type, TypeInfo>();
        internal readonly Type type;
        readonly ConstructorInfo[] constructors;
        internal readonly PropertyInfo[] indexers;
        readonly IReadOnlyDictionary<(string, MemberType), MemberInfo> members;
        readonly IReadOnlyDictionary<string, MethodInfo[]> methods;
        readonly IReadOnlyDictionary<(Type, MemberType), MethodInfo> castOperators;

        public static TypeInfo Get(Type type) {
            if (!cache.TryGetValue(type, out var typeInfo))
                cache[type] = typeInfo = new TypeInfo(type);
            return typeInfo;
        }

        TypeInfo(Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            this.type = type;
            var tempMembers = new Dictionary<(string, MemberType), MemberInfo>();
            var methods = new Dictionary<string, MethodInfo[]>();
            var castOperators = new Dictionary<(Type, MemberType), MethodInfo>();
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
                        tempMembers[(member.Name, MemberType.Event)] = member as EventInfo;
                        break;
                    }
                    case MemberTypes.Field: {
                        tempMembers[(member.Name, MemberType.Field)] = member as FieldInfo;
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
                                        castOperators[(inputType, MemberType.CastInOperator)] = method;
                                    else if (inputType == type)
                                        castOperators[(returnType, MemberType.CastOutOperator)] = method;
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
                        tempMembers[(property.Name, MemberType.Property)] = property;
                        break;
                    }
                    case MemberTypes.NestedType: {
                        tempMembers[(member.Name, MemberType.NestedType)] = member as Type;
                        break;
                    }
                }
            foreach (var pair in tempMethods) methods[pair.Key] = pair.Value.ToArray();
            indexers = tempIndexers.ToArray();
            constructors = tempConstructors.ToArray();
            #if NET8_0_OR_GREATER
            members = tempMembers.ToFrozenDictionary();
            this.castOperators = castOperators.ToFrozenDictionary();
            this.methods = methods.ToFrozenDictionary();
            #else
            members = tempMembers;
            this.castOperators = castOperators;
            this.methods = methods;
            #endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryGet<T>(string name, MemberType memberType, out T member) where T : MemberInfo {
            if (members.TryGetValue((name, memberType), out var value)) {
                member = value as T;
                return true;
            }
            member = null;
            return false;
        }

        internal bool TryGetValue(object instance, string key, out object value) {
            if (TryGet(key, MemberType.Property, out PropertyInfo property) && property.CanRead) {
                value = InternalWrap(property.GetValue(instance));
                return true;
            }
            if (TryGet(key, MemberType.Field, out FieldInfo field)) {
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
            if (TryGet(key, MemberType.Property, out PropertyInfo property) && property.CanWrite) {
                property.SetValue(InternalUnwrap(instance), value);
                return true;
            }
            if (TryGet(key, MemberType.Field, out FieldInfo field) && !field.IsInitOnly) {
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

        internal bool TryGetProperties(string propertyName, out PropertyInfo property) => TryGet(propertyName, MemberType.Property, out property);

        internal bool TryGetConverter(Type type, bool isIn, out MethodInfo method) =>
            castOperators.TryGetValue((type, isIn ? MemberType.CastInOperator : MemberType.CastOutOperator), out method);

        internal bool TryInvoke(object instance, string methodName, object[] args, out object result, CallInfo callInfo = null) {
            var safeArgs = args ?? Array.Empty<object>();
            if (methods.TryGetValue(methodName, out var methodArrays) &&
                TryGetMatchingMethod(methodArrays, ref safeArgs, out var resultMethod, callInfo, out var bindState)) {
                result = InternalWrap(resultMethod.Invoke(resultMethod.IsStatic ? null : InternalUnwrap(instance), safeArgs));
                InternalWrap(safeArgs, args, bindState, callInfo?.ArgumentNames.Count ?? 0);
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
            if (TryGetMatchingMethod(constructors, ref args, out var resultConstructor, null, out _)) {
                result = InternalWrap(resultConstructor.Invoke(args));
                return true;
            }
            result = null;
            return false;
        }

        internal bool TryGetSubType(string name, out Type type) => TryGet(name, MemberType.NestedType, out type);

        internal bool TryGetEvent(string name, out EventInfo evt) => TryGet(name, MemberType.Event, out evt);

        public bool Equals(TypeInfo other) => type == other.type;

        public override bool Equals(object obj) => obj is TypeInfo other && Equals(other);

        public override int GetHashCode() => type?.GetHashCode() ?? 0;

        public override string ToString() => type?.ToString() ?? "(empty)";
    }

    enum MemberType : byte {
        Event,
        Field,
        Property,
        NestedType,
        CastInOperator,
        CastOutOperator,
    }
}
