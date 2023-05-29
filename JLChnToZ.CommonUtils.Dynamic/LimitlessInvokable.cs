using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Special dynamic object that can be used to invoke methods with same name but different signatures.</summary>
    /// <remarks>This should not be directly used. Use <see cref="Limitless"/> instead.</remarks>
    public class LimitlessInvokable: DynamicObject {
        readonly MethodInfo[] methodInfos;
        readonly object target;

        internal LimitlessInvokable(object target, MethodInfo[] methodInfos) {
            if (methodInfos == null || methodInfos.Length == 0)
                throw new ArgumentNullException(nameof(methodInfos));
            this.target = target;
            this.methodInfos = methodInfos;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            switch (binder.Name) {
                case "of":
                    var types = new Type[args.Length];
                    for (int i = 0; i < args.Length; i++) {
                        var typeLike = args[i];
                        if (typeLike is Type type) {
                            types[i] = type;
                            continue;
                        }
                        if (typeLike is string typeName) {
                            types[i] = Type.GetType(typeName);
                            continue;
                        }
                        if (typeLike is Limitless limitless) {
                            if (limitless.target is Type type2) {
                                types[i] = type2;
                                continue;
                            }
                            if (limitless.target == null) {
                                types[i] = limitless.type;
                                continue;
                            }
                        }
                        goto fail;
                    }
                    result = ResolveGenerics(types);
                    return true;
            }
            fail:
            result = null;
            return false;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) =>
            TryInvoke(args, out result);

        bool TryInvoke(object[] args, out object result, IList<Type> genericTypes = null) {
            var safeArgs = args;
            if (TryGetMatchingMethod(methodInfos, ref safeArgs, out var methodInfo, genericTypes)) {
                result = InternalWrap(methodInfo.Invoke(target, safeArgs));
                InternalWrap(safeArgs, args);
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>Invokes the method with the given arguments.</summary>
        public dynamic Invoke(params object[] args) {
            if (TryInvoke(args, out var result)) return result;
            throw new InvalidOperationException("No matching method found.");
        }

        public override bool TryConvert(ConvertBinder binder, out object result) =>
            TryCreateDelegate(binder.Type, out result);

        /// <summary>Creates a delegate with the given type.</summary>
        public dynamic CreateDelegate(Type delegateType) {
            if (TryCreateDelegate(delegateType, out var result)) return result;
            throw new InvalidOperationException("No matching method found.");
        }

        bool TryCreateDelegate(Type delegateType, out object result) {
            if (delegateType.IsSubclassOf(typeof(Delegate))) {
                var invokeMethod = delegateType.GetMethod("Invoke");
                if (invokeMethod != null) {
                    var expectedParameters = invokeMethod.GetParameters();
                    var expectedReturnType = invokeMethod.ReturnType;
                    foreach (var methodInfo in methodInfos) {
                        if (methodInfo.ContainsGenericParameters ||
                            methodInfo.ReturnType != expectedReturnType) continue;
                        var parameters = methodInfo.GetParameters();
                        if (parameters.Length != expectedParameters.Length) continue;
                        for (int i = 0; i < parameters.Length; i++)
                            if (parameters[i].ParameterType != expectedParameters[i].ParameterType)
                                goto NoMatches;
                        result = target == null ?
                            Delegate.CreateDelegate(delegateType, methodInfo, false) :
                            Delegate.CreateDelegate(delegateType, target, methodInfo, false);
                        if (result != null) return true;
                        NoMatches:;
                    }
                }
            }
            result = null;
            return false;
        }

        /// <summary>Fulfills generic parameters of the methods.</summary>
        public dynamic Of(params Type[] types) => ResolveGenerics(types);

        LimitlessInvokable ResolveGenerics(Type[] types) {
            if (types == null) throw new ArgumentNullException(nameof(types));
            var filteredMethods = new List<MethodInfo>();
            foreach (var methodInfo in methodInfos) {
                var genericParams = methodInfo.GetGenericArguments();
                if (genericParams.Length != types.Length) continue;
                try {
                    filteredMethods.Add(types.Length > 0 ? methodInfo.MakeGenericMethod(types) : methodInfo);
                } catch (Exception ex) {
                    throw ex;
                } // Try next if it fails
            }
            return new LimitlessInvokable(target, filteredMethods.ToArray());
        }
    }
}
