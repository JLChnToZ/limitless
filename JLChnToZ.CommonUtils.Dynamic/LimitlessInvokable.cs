using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Special dynamic object that can be used to invoke methods with same name but different signatures.</summary>
    /// <remarks>This should not be directly used. Use <see cref="Limitless"/> instead.</remarks>
    public class LimitlessInvokable: DynamicObject {
        protected readonly MethodInfo[] methodInfos;
        protected readonly object target;

        protected virtual MethodInfo[] MethodInfos => methodInfos;

        protected virtual object InvokeTarget => target;

        internal LimitlessInvokable(object target, MethodInfo[] methodInfos) {
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
            TryInvoke(args, out result, binder.CallInfo);

        bool TryInvoke(object[] args, out object result, CallInfo callInfo = null) {
            var safeArgs = args;
            if (TryGetMatchingMethod(MethodInfos, ref safeArgs, out var methodInfo, callInfo, out var bindState)) {
                result = InternalWrap(methodInfo.Invoke(InvokeTarget, safeArgs));
                InternalWrap(safeArgs, args, bindState, callInfo?.ArgumentNames.Count ?? 0);
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

        public override bool TryConvert(ConvertBinder binder, out object result) {
            if (target != null && binder.Type.IsAssignableFrom(target.GetType())) {
                result = target;
                return true;
            }
            if (TryCreateDelegate(binder.Type, out var resultDelegate)) {
                result = resultDelegate;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>Creates a delegate with the given type.</summary>
        public Delegate CreateDelegate(Type delegateType) {
            if (TryCreateDelegate(delegateType, out var result)) return result;
            throw new InvalidOperationException("No matching method found.");
        }

        public T CreateDelegate<T>() where T : Delegate => CreateDelegate(typeof(T)) as T;

        internal protected bool TryCreateDelegate(Type delegateType, out Delegate result) {
            if (delegateType.IsSubclassOf(typeof(Delegate)) &&
                TryGetDelegateSignature(delegateType, out var parameters, out var returnType)) {
                var matched = SelectMethod(MethodInfos, parameters);
                if (matched != null && matched.ReturnType == returnType) {
                    var target = InvokeTarget;
                    if (matched.IsStatic) {
                        result = Delegate.CreateDelegate(delegateType, matched, false);
                        return result != null;
                    }
                    var declaringType = matched.DeclaringType;
                    if (declaringType.IsSubclassOf(typeof(Delegate)) && matched.Name == "Invoke") {
                        result = (target as Delegate).ConvertAndFlatten(delegateType);
                        return result != null;
                    }
                    result = Delegate.CreateDelegate(delegateType, target, matched, false);
                    return result != null;
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
            foreach (var methodInfo in MethodInfos) {
                var genericParams = methodInfo.GetGenericArguments();
                if (genericParams.Length != types.Length) continue;
                try {
                    filteredMethods.Add(types.Length > 0 ? methodInfo.MakeGenericMethod(types) : methodInfo);
                } catch (Exception ex) {
                    throw ex;
                } // Try next if it fails
            }
            return new LimitlessInvokable(InvokeTarget, filteredMethods.ToArray());
        }
    }
}
