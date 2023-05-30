using System;
using System.Linq.Expressions;
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

        public static object InternalWrap(object obj, Type type = null) =>
            obj == null || obj is Limitless || obj is LimitlessInvokable || Type.GetTypeCode(obj.GetType()) != TypeCode.Object ? obj : new Limitless(obj, type);

        public static void InternalWrap(object[] sourceObj, object[] destObj) {
            if (sourceObj == null || destObj == null) return;
            if (sourceObj != destObj) Array.Copy(sourceObj, 0, destObj, 0, Math.Min(sourceObj.Length, destObj.Length));
            for (int i = 0; i < destObj.Length; i++) destObj[i] = InternalWrap(destObj[i]);
        }

        public static bool TryGetMatchingMethod<T>(T[] methodInfos, ref object[] args, out T bestMatches) where T : MethodBase {
            MethodBase matched;
            if (args == null) args = emptyArgs;
            var binder = Type.DefaultBinder;
            matched = binder.SelectMethod(DEFAULT_FLAGS, methodInfos, Array.ConvertAll(args, GetUndelyType), null);
            if (matched != null) {
                bestMatches = (T)matched;
                args = InternalUnwrap(args, binder, Array.ConvertAll(matched.GetParameters(), GetParameterType));
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

        public static string ToOperatorMethodName(this ExpressionType expressionType) {
            switch (expressionType) {
                case ExpressionType.Equal: return "op_Equality";
                case ExpressionType.NotEqual: return "op_Inequality";
                case ExpressionType.GreaterThan: return "op_GreaterThan";
                case ExpressionType.LessThan: return "op_LessThan";
                case ExpressionType.GreaterThanOrEqual: return "op_GreaterThanOrEqual";
                case ExpressionType.LessThanOrEqual: return "op_LessThanOrEqual";
                case ExpressionType.Add: case ExpressionType.AddAssign: return "op_Addition";
                case ExpressionType.Subtract: case ExpressionType.SubtractAssign: return "op_Subtraction";
                case ExpressionType.Multiply: case ExpressionType.MultiplyAssign: return "op_Multiply";
                case ExpressionType.Divide: case ExpressionType.DivideAssign: return "op_Division";
                case ExpressionType.Modulo: case ExpressionType.ModuloAssign: return "op_Modulus";
                case ExpressionType.And: case ExpressionType.AndAssign: return "op_BitwiseAnd";
                case ExpressionType.Or: case ExpressionType.OrAssign: return "op_BitwiseOr";
                case ExpressionType.ExclusiveOr: case ExpressionType.ExclusiveOrAssign: return "op_ExclusiveOr";
                case ExpressionType.LeftShift: case ExpressionType.LeftShiftAssign: return "op_LeftShift";
                case ExpressionType.RightShift: case ExpressionType.RightShiftAssign: return "op_RightShift";
                case ExpressionType.Negate: return "op_UnaryNegation";
                case ExpressionType.Not: return "op_LogicalNot";
                case ExpressionType.OnesComplement: return "op_OnesComplement";
                case ExpressionType.Increment: return "op_Increment";
                case ExpressionType.Decrement: return "op_Decrement";
                default: throw new NotSupportedException();
            }
        }
    }

    internal enum MethodMatchLevel {
        NotMatch,
        Implicit,
        Exact,
    }
}
