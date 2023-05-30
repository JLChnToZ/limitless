using System;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    internal static class Utilites {
        public const BindingFlags BASE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const BindingFlags STATIC_FLAGS = BASE_FLAGS | BindingFlags.Static;
        public const BindingFlags INSTANCE_FLAGS = BASE_FLAGS | BindingFlags.Instance;
        public const BindingFlags DEFAULT_FLAGS = STATIC_FLAGS | INSTANCE_FLAGS;
        public static readonly object[] emptyArgs = new object[0];

        public static object InternalUnwrap(object obj, Binder binder = null, Type requestedType = null) {
            if (obj == null) return null;
            if (obj is Limitless limitObj) obj = limitObj.target;
            if (requestedType != null) {
                try {
                    if (binder != null) return binder.ChangeType(obj, requestedType, null);
                } catch {}
                try {
                    return Convert.ChangeType(obj, requestedType);
                } catch {}
            }
            return obj;
        }

        public static object[] InternalUnwrap(object[] objs, Binder binder = null, Type[] requestedTypes = null) {
            for (int i = 0; i < objs.Length; i++) objs[i] = InternalUnwrap(objs[i], binder, requestedTypes != null ? requestedTypes[i] : null);
            return objs;
        }

        public static object InternalWrap(object obj) =>
            obj == null || obj is Limitless || obj is LimitlessInvokable || Type.GetTypeCode(obj.GetType()) != TypeCode.Object ? obj : new Limitless(obj);

        public static void InternalWrap(object[] sourceObj, object[] destObj) {
            if (sourceObj == null || destObj == null) return;
            if (sourceObj != destObj) Array.Copy(sourceObj, 0, destObj, 0, Math.Min(sourceObj.Length, destObj.Length));
            for (int i = 0; i < destObj.Length; i++) destObj[i] = InternalWrap(destObj[i]);
        }

        public static bool TryGetMatchingMethod<T>(T[] methodInfos, ref object[] args, out T bestMatches) where T : MethodBase {
            MethodBase matched;
            if (args == null) args = emptyArgs;
            matched = Type.DefaultBinder.SelectMethod(DEFAULT_FLAGS, methodInfos, Array.ConvertAll(args, GetUndelyType), null);
            if (matched != null) {
                bestMatches = (T)matched;
                args = InternalUnwrap(args, Type.DefaultBinder, Array.ConvertAll(matched.GetParameters(), GetParameterType));
                return true;
            }
            bestMatches = null;
            return false;
        }

        public static Type GetUndelyType(object obj) {
            if (obj == null) return typeof(object);
            if (obj is Limitless limitObj) return limitObj.type;
            return obj.GetType();
        }

        public static Type GetParameterType(ParameterInfo parameterInfo) => parameterInfo.ParameterType;
    }

    internal enum MethodMatchLevel {
        NotMatch,
        Implicit,
        Exact,
    }
}
