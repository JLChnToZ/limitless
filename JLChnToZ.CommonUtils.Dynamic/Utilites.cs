using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    internal static class Utilites {
        public const BindingFlags BASE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const BindingFlags STATIC_FLAGS = BASE_FLAGS | BindingFlags.Static;
        public const BindingFlags INSTANCE_FLAGS = BASE_FLAGS | BindingFlags.Instance;
        public const BindingFlags DEFAULT_FLAGS = STATIC_FLAGS | INSTANCE_FLAGS;
        public static readonly object[] emptyArgs = new object[0];

        public static object InternalUnwrap(object obj, Type requestedType = null) {
            if (obj == null) return null;
            if (obj is Limitless limitObj) obj = limitObj.target;
            if (requestedType != null) {
                try {
                    return Type.DefaultBinder.ChangeType(obj, requestedType, null);
                } catch {}
                try {
                    return Convert.ChangeType(obj, requestedType);
                } catch {}
            }
            return obj;
        }

        public static object[] InternalUnwrap(object[] args, ParameterInfo[] parameterInfos = null) {
            for (int i = 0; i < args.Length; i++) args[i] = InternalUnwrap(args[i], parameterInfos?[i]?.ParameterType);
            return args;
        }

        public static object InternalWrap(object obj, Type type = null) =>
            obj == null || obj is Limitless || obj is LimitlessInvokable || Type.GetTypeCode(obj.GetType()) != TypeCode.Object ? obj : new Limitless(obj, type);

        public static void InternalWrap(object[] sourceObj, object[] destObj) {
            if (sourceObj == null || destObj == null) return;
            if (sourceObj != destObj) Array.Copy(sourceObj, 0, destObj, 0, Math.Min(sourceObj.Length, destObj.Length));
            for (int i = 0; i < destObj.Length; i++) destObj[i] = InternalWrap(destObj[i]);
        }

        public static bool TryGetMatchingMethod<T>(T[] methodInfos, ref object[] args, out T bestMatches) where T : MethodBase {
            if (args == null) args = emptyArgs;
            bestMatches = SelectMethod(methodInfos, Array.ConvertAll(args, GetUndelyType));
            if (bestMatches != null) {
                args = InternalUnwrap(args, bestMatches.GetParameters());
                return true;
            }
            return false;
        }

        public static TMethod SelectMethod<TMethod>(TMethod[] methodInfos, Type[] parameters, BindingFlags bindingFlags = DEFAULT_FLAGS) where TMethod : MethodBase =>
            Type.DefaultBinder.SelectMethod(bindingFlags, methodInfos, parameters, null) as TMethod;

        public static PropertyInfo SelectIndexer(PropertyInfo[] indexers, Type[] parameters, Type returnType = null, BindingFlags bindingFlags = DEFAULT_FLAGS) =>
            Type.DefaultBinder.SelectProperty(bindingFlags, indexers, returnType, parameters, null);

        public static Type GetUndelyType(object obj) {
            if (obj == null) return typeof(object);
            if (obj is Limitless limitObj) return limitObj.type;
            return obj.GetType();
        }

        public static bool TryGetDelegateSignature(Type delegateType, out Type[] parameters, out Type returnType) {
            if (delegateType == null) {
                parameters = null;
                returnType = null;
                return false;
            }
            var invokeMethod = delegateType.GetMethod("Invoke");
            if (invokeMethod == null) {
                parameters = null;
                returnType = null;
                return false;
            }
            parameters = Array.ConvertAll(invokeMethod.GetParameters(), GetParameterType);
            returnType = invokeMethod.ReturnType;
            return true;
        }

        static Type GetParameterType(ParameterInfo parameterInfo) => parameterInfo.ParameterType;

        public static string ToOperatorMethodName(this ExpressionType expressionType) {
            switch (expressionType) {
                case ExpressionType.Equal: return "op_Equality";
                case ExpressionType.NotEqual: return "op_Inequality";
                case ExpressionType.GreaterThan: return "op_GreaterThan";
                case ExpressionType.LessThan: return "op_LessThan";
                case ExpressionType.GreaterThanOrEqual: return "op_GreaterThanOrEqual";
                case ExpressionType.LessThanOrEqual: return "op_LessThanOrEqual";
                case ExpressionType.Add: case ExpressionType.AddChecked:
                case ExpressionType.AddAssign: case ExpressionType.AddAssignChecked: return "op_Addition";
                case ExpressionType.Subtract: case ExpressionType.SubtractChecked:
                case ExpressionType.SubtractAssign: case ExpressionType.SubtractAssignChecked: return "op_Subtraction";
                case ExpressionType.Multiply: case ExpressionType.MultiplyChecked:
                case ExpressionType.MultiplyAssign: case ExpressionType.MultiplyAssignChecked: return "op_Multiply";
                case ExpressionType.Divide: case ExpressionType.DivideAssign: return "op_Division";
                case ExpressionType.Modulo: case ExpressionType.ModuloAssign: return "op_Modulus";
                case ExpressionType.And: case ExpressionType.AndAssign: return "op_BitwiseAnd";
                case ExpressionType.Or: case ExpressionType.OrAssign: return "op_BitwiseOr";
                case ExpressionType.ExclusiveOr: case ExpressionType.ExclusiveOrAssign: return "op_ExclusiveOr";
                case ExpressionType.LeftShift: case ExpressionType.LeftShiftAssign: return "op_LeftShift";
                case ExpressionType.RightShift: case ExpressionType.RightShiftAssign: return "op_RightShift";
                case ExpressionType.UnaryPlus: return "op_UnaryPlus";
                case ExpressionType.Negate: case ExpressionType.NegateChecked: return "op_UnaryNegation";
                case ExpressionType.Not: return "op_LogicalNot";
                case ExpressionType.OnesComplement: return "op_OnesComplement";
                case ExpressionType.Increment: return "op_Increment";
                case ExpressionType.Decrement: return "op_Decrement";
                case ExpressionType.Convert: case ExpressionType.ConvertChecked: return "op_Explicit";
                case ExpressionType.IsTrue: return "op_True";
                case ExpressionType.IsFalse: return "op_False";
                default: return ""; // Fail silently
            }
        }

        public static int GetOperatorParameterCount(this ExpressionType expressionType) {
            switch (expressionType) {
                case ExpressionType.Equal: case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan: case ExpressionType.LessThan:
                case ExpressionType.GreaterThanOrEqual: case ExpressionType.LessThanOrEqual:
                case ExpressionType.Add: case ExpressionType.AddChecked:
                case ExpressionType.AddAssign: case ExpressionType.AddAssignChecked:
                case ExpressionType.Subtract: case ExpressionType.SubtractChecked:
                case ExpressionType.SubtractAssign: case ExpressionType.SubtractAssignChecked:
                case ExpressionType.Multiply: case ExpressionType.MultiplyChecked:
                case ExpressionType.MultiplyAssign: case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.Divide: case ExpressionType.DivideAssign:
                case ExpressionType.Modulo: case ExpressionType.ModuloAssign:
                case ExpressionType.And: case ExpressionType.AndAssign:
                case ExpressionType.Or: case ExpressionType.OrAssign:
                case ExpressionType.ExclusiveOr: case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShift: case ExpressionType.LeftShiftAssign:
                case ExpressionType.RightShift: case ExpressionType.RightShiftAssign:
                case ExpressionType.Power: case ExpressionType.PowerAssign:
                    return 2;
                case ExpressionType.UnaryPlus: case ExpressionType.Negate: case ExpressionType.NegateChecked:
                case ExpressionType.Not: case ExpressionType.OnesComplement:
                case ExpressionType.Increment: case ExpressionType.Decrement:
                case ExpressionType.Convert: case ExpressionType.ConvertChecked:
                case ExpressionType.IsTrue: case ExpressionType.IsFalse:
                    return 1;
                default: return 0;
            }
        }

        public static bool TryInvokeSafe(this object obj, bool isStatic, string methodName, out object result, params object[] args) {
            if (obj == null) {
                result = null;
                return false;
            }
            if (obj is Limitless limitObj)
                return limitObj.TryInvoke(isStatic, methodName, out result, args);
            return TypeInfo.Get(obj.GetType()).TryInvoke(isStatic ? null : obj, methodName, args, out result);
        }

        public static MethodInfo GetUndelyRaiseMethod(this EventInfo eventInfo, out Delegate backingDelegate, object instance = null) {
            var raiseMethod = eventInfo.GetRaiseMethod(true);
            backingDelegate = null;
            if (raiseMethod == null) {
                var instanceType = instance?.GetType() ?? eventInfo.DeclaringType;
                var backingField = instanceType.GetField(eventInfo.Name, DEFAULT_FLAGS);
                if (backingField != null) {
                    var backingFieldType = backingField.FieldType;
                    if (backingFieldType.IsSubclassOf(typeof(Delegate))) {
                        backingDelegate = backingField.GetValue(instance) as Delegate;
                        if (backingDelegate != null) raiseMethod = backingFieldType.GetMethod("Invoke");
                    }
                }
            }
            return raiseMethod;
        }

        public static Delegate ConvertAndFlatten(this Delegate del, Type toType = null) {
            if (del == null) return null;
            if (toType == null) toType = del.GetType();
            var entries = new Stack<Delegate>();
            entries.Push(del);
            del = null;
            while (entries.Count > 0) {
                var current = entries.Pop();
                if (current == null) continue;
                // Check if current delegate multi-cast and if so, flatten it
                var list = current.GetInvocationList();
                if (list.Length > 1) {
                    // We do it in reverse order to preserve the order of the delegates
                    for (int i = list.Length - 1; i >= 0; i--)
                        entries.Push(list[i]);
                    continue;
                }
                var target = current.Target;
                var method = current.Method;
                // Check if target is a delegate and if so, flatten it
                if (target is Delegate subDelegate && method.Name == "Invoke") {
                    list = subDelegate.GetInvocationList();
                    for (int i = list.Length - 1; i >= 0; i--)
                        entries.Push(list[i]);
                    continue;
                }
                if (current.GetType() != toType)
                    current = method.IsStatic ? Delegate.CreateDelegate(toType, method, false) :
                        Delegate.CreateDelegate(toType, target, method, false);
                del = Delegate.Combine(del, current);
            }
            return del;
        }
    }
}
