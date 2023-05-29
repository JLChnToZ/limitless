using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    internal static class Utilites {
        public const BindingFlags BASE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const BindingFlags STATIC_FLAGS = BASE_FLAGS | BindingFlags.Static;
        public const BindingFlags INSTANCE_FLAGS = BASE_FLAGS | BindingFlags.Instance;
        public const BindingFlags DEFAULT_FLAGS = STATIC_FLAGS | INSTANCE_FLAGS;
        public static readonly object[] emptyArgs = new object[0];

        public static object InternalUnwrap(object obj) => obj is Limitless limitObj ? limitObj.target : obj;

        public static object[] InternalUnwrap(object[] objs) {
            for (int i = 0; i < objs.Length; i++) objs[i] = InternalUnwrap(objs[i]);
            return objs;
        }

        public static MethodMatchLevel UnwrapParamsAndCheck(ParameterInfo[] paramInfos, ref object[] input, bool shouldCloneArray = false) {
            if (input == null) input = emptyArgs;
            var currentMatchLevel = MethodMatchLevel.Exact;
            if (paramInfos.Length > input.Length) {
                var newInput = new object[paramInfos.Length];
                Array.Copy(input, newInput, input.Length);
                input = newInput;
                shouldCloneArray = true;
                currentMatchLevel = MethodMatchLevel.Implicit;
            }
            if (input.Length > paramInfos.Length && paramInfos.Length == 0)
                return MethodMatchLevel.NotMatch;
            for (int i = 0; i < paramInfos.Length; i++) {
                var inputObj = input[i];
                var matchLevel = UnwrapParamAndCheck(paramInfos[i].ParameterType, ref inputObj);
                if (matchLevel == MethodMatchLevel.NotMatch) return MethodMatchLevel.NotMatch;
                input[i] = inputObj;
                if (matchLevel < currentMatchLevel) currentMatchLevel = matchLevel;
            }
            if (currentMatchLevel != MethodMatchLevel.Exact && !shouldCloneArray) {
                var newInput = new object[paramInfos.Length];
                Array.Copy(input, newInput, input.Length);
                input = newInput;
            }
            return currentMatchLevel;
        }

        public static MethodMatchLevel UnwrapParamAndCheck(Type type, ref object input) {
            if (input == null) return type.IsValueType ? MethodMatchLevel.NotMatch : MethodMatchLevel.Implicit;
            Type inputType, preferredType;
            if (input is Limitless limitObj) {
                input = limitObj.target;
                preferredType = limitObj.type;
                inputType = input.GetType();
            } else
                inputType = preferredType = input.GetType();
            return type == preferredType ? MethodMatchLevel.Exact :
                type.IsAssignableFrom(inputType) ? MethodMatchLevel.Implicit :
                MethodMatchLevel.NotMatch;
        }

        public static object InternalWrap(object obj) =>
            obj == null || obj is DynamicObject || Type.GetTypeCode(obj.GetType()) != TypeCode.Object ? obj : new Limitless(obj);

        public static void InternalWrap(object[] sourceObj, object[] destObj) {
            if (sourceObj == null || destObj == null) return;
            if (sourceObj != destObj) Array.Copy(sourceObj, 0, destObj, 0, Math.Min(sourceObj.Length, destObj.Length));
            for (int i = 0; i < destObj.Length; i++) destObj[i] = InternalWrap(destObj[i]);
        }

        public static bool TryGetMatchingMethod<T>(T[] methodInfos, ref object[] args, out T bestMatches, IList<Type> genericTypes = null) where T : MethodBase {
            (T method, object[] safeArgs)? fallback = null;
            foreach (var method in methodInfos) {
                var m = method;
                var safeArgs = args;
                if (method.ContainsGenericParameters) {
                    var genericArgs = method.GetGenericArguments();
                    if (genericTypes == null || method.MemberType != MemberTypes.Method || genericArgs.Length != genericTypes.Count) continue;
                    try {
                        var typeArgsArray = new Type[genericTypes.Count];
                        genericTypes.CopyTo(typeArgsArray, 0);
                        m = (method as MethodInfo).MakeGenericMethod(typeArgsArray) as T;
                    } catch {
                        continue;
                    }
                } else if (genericTypes != null && genericTypes.Count > 0) continue;
                switch (UnwrapParamsAndCheck(m.GetParameters(), ref safeArgs)) {
                    case MethodMatchLevel.Exact:
                        fallback = (m, safeArgs);
                        goto skip;
                    case MethodMatchLevel.Implicit:
                        if (fallback == null) fallback = (m, safeArgs);
                        break;
                }
            }
            skip: if (fallback.HasValue) {
                bestMatches = fallback.Value.method;
                args = fallback.Value.safeArgs;
                return true;
            }
            bestMatches = null;
            return false;
        }
    }

    internal enum MethodMatchLevel {
        NotMatch,
        Implicit,
        Exact,
    }
}
