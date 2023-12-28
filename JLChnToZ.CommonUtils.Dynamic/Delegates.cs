using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>
    /// Provides methods to bind methods, properties, converters and operators to delegates, regardness of its visibility.
    /// </summary>
    public static class Delegates {
        static readonly Dictionary<(Type srcType, Type destType, Type delegateType), Delegate> converterCache =
            new Dictionary<(Type, Type, Type), Delegate>();
        static readonly Dictionary<(Type lhs, Type rhs, Type result, ExpressionType op, Type delegateType), Delegate> operatorCache =
            new Dictionary<(Type, Type, Type, ExpressionType, Type), Delegate>();

        #region Bind Method & Property
        /// <summary>Bind a method to a delegate.</summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="typeName">The type name of the type that contains the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The delegate that bound to the method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeName"/> or <paramref name="methodName"/> is null or empty.</exception>
        public static TDelegate BindMethod<TDelegate>(string typeName, string methodName) where TDelegate : Delegate {
            if (string.IsNullOrEmpty(typeName)) throw new ArgumentNullException(nameof(typeName));
            var type = Type.GetType(typeName, false);
            return type != null ? BindMethod<TDelegate>(type, methodName) : null;
        }

        /// <summary>Bind a method to a delegate.</summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="type">The type that contains the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The delegate that bound to the method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="methodName"/> is null or empty.</exception>
        public static TDelegate BindMethod<TDelegate>(Type type, string methodName) where TDelegate : Delegate {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
            return BindMethod(type, methodName, STATIC_FLAGS, typeof(TDelegate)) as TDelegate;
        }

        /// <summary>Bind a method to a delegate.</summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="target">The object that contains the method.</param>
        /// <returns>The delegate that bound to the method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="methodName"/> is null or empty.</exception>
        public static TDelegate BindMethod<TDelegate>(string methodName, object target) where TDelegate : Delegate {
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return BindMethod(target.GetType(), methodName, DEFAULT_FLAGS, typeof(TDelegate), target) as TDelegate;
        }

        /// <summary>Bind a method to a delegate.</summary>
        /// <typeparam name="T">The type that contains the method.</typeparam>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="target">The object that contains the method.</param>
        /// <returns>The delegate that bound to the method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodName"/> is null or empty.</exception>
        public static TDelegate BindMethod<T, TDelegate>(string methodName, T target = default) where TDelegate : Delegate {
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
            return BindMethod(typeof(T), methodName, DEFAULT_FLAGS, typeof(TDelegate), target) as TDelegate;
        }

        /// <summary>Bind a property to a delegate.</summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="typeName">The type name of the type that contains the property.</param>
        /// <param name="propertyName">The name of the property. Leave empty if you want to bind indexer.</param>
        /// <returns>The delegate that bound to the property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeName"/> is null or empty.</exception>
        /// <remarks>It will binds getter or setter depends on the return type of the delegate.</remarks>
        public static TDelegate BindProperty<TDelegate>(string typeName, string propertyName = "") where TDelegate : Delegate {
            if (string.IsNullOrEmpty(typeName)) throw new ArgumentNullException(nameof(typeName));
            var type = Type.GetType(typeName, false);
            return type != null ? BindProperty<TDelegate>(type, propertyName) : null;
        }

        /// <summary>Bind a property to a delegate.</summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="type">The type that contains the property.</param>
        /// <param name="propertyName">The name of the property. Leave empty if you want to bind indexer.</param>
        /// <returns>The delegate that bound to the property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <remarks>It will binds getter or setter depends on the return type of the delegate.</remarks>
        public static TDelegate BindProperty<TDelegate>(Type type, string propertyName = "") where TDelegate : Delegate {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return BindProperty(type, DEFAULT_FLAGS, propertyName, typeof(TDelegate)) as TDelegate;
        }

        /// <summary>Bind a property to a delegate.</summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="propertyName">The name of the property. Leave empty if you want to bind indexer.</param>
        /// <param name="target">The object that contains the property.</param>
        /// <returns>The delegate that bound to the property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is null.</exception>
        /// <remarks>It will binds getter or setter depends on the return type of the delegate.</remarks>
        public static TDelegate BindProperty<TDelegate>(object target, string propertyName = "") where TDelegate : Delegate {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return BindProperty(target.GetType(), DEFAULT_FLAGS, propertyName, typeof(TDelegate), target) as TDelegate;
        }

        /// <summary>Bind a property to a delegate.</summary>
        /// <typeparam name="T">The type that contains the property.</typeparam>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="propertyName">The name of the property. Leave empty if you want to bind indexer.</param>
        /// <param name="target">The object that contains the property.</param>
        /// <returns>The delegate that bound to the property.</returns>
        /// <remarks>It will binds getter or setter depends on the return type of the delegate.</remarks>
        public static TDelegate BindProperty<T, TDelegate>(T target = default, string propertyName = "") where TDelegate : Delegate =>
            BindProperty(typeof(T), DEFAULT_FLAGS, propertyName, typeof(TDelegate), target) as TDelegate;

        static Delegate BindMethod(Type type, string methodName, BindingFlags bindingFlags, Type delegateType, object target = null) =>
            TypeInfo.Get(type).TryGetMethods(methodName, out var methods) ? BindMethod(
                type, delegateType,
                Type.DefaultBinder.SelectMethod(
                    bindingFlags, methods,
                    Array.ConvertAll(
                        delegateType.GetMethod("Invoke").GetParameters(),
                        GetParameterType
                    ),
                    null
                ) as MethodInfo,
                target
            ) : null;

        static Delegate BindProperty(Type type, BindingFlags bindingFlags, string propertyName, Type delegateType, object target = null) {
            var invokeMethod = delegateType.GetMethod("Invoke");
            var returnType = invokeMethod.ReturnType;
            var parameters = Array.ConvertAll(invokeMethod.GetParameters(), GetParameterType);
            bool isSetterRequested = returnType == typeof(void);
            if (isSetterRequested) {
                if (parameters.Length == 0) return null;
                returnType = parameters[parameters.Length - 1];
                Array.Resize(ref parameters, parameters.Length - 1);
            }
            var typeInfo = TypeInfo.Get(type);
            PropertyInfo selectedProperty;
            if (parameters.Length > 0) {
                selectedProperty = Type.DefaultBinder.SelectProperty(bindingFlags, typeInfo.indexers, returnType, parameters, null);
                if (selectedProperty == null) return null;
            } else if (!typeInfo.TryGetProperties(propertyName, out selectedProperty))
                return null;
            return BindMethod(
                type, delegateType,
                isSetterRequested ?
                    selectedProperty.GetSetMethod(true) :
                    selectedProperty.GetGetMethod(true),
                target
            );
        }

        static Delegate BindMethod(Type targetType, Type delegateType, MethodInfo method, object target) {
            if (method == null) return null;
            if (method.IsStatic) return Delegate.CreateDelegate(delegateType, method, false);
            if (target == null) {
                if (!targetType.IsValueType) return null;
                target = Activator.CreateInstance(targetType);
            }
            return Delegate.CreateDelegate(delegateType, target, method, false);
        }

        static Type GetParameterType(ParameterInfo parameter) => parameter.ParameterType;
        #endregion

        #region Bind Converters
        /// <summary>Bind a converter to a delegate.</summary>
        /// <typeparam name="TSource">The source type of the converter.</typeparam>
        /// <typeparam name="TDest">The destination type of the converter.</typeparam>
        /// <returns>The <see cref="Converter{TSource, TDest}"/> that bound to the converter.</returns>
        public static Converter<TSource, TDest> BindConverter<TSource, TDest>() =>
            BindConverterUnchecked(typeof(TSource), typeof(TDest), typeof(Converter<TSource, TDest>)) as Converter<TSource, TDest>;

        /// <summary>Bind a converter to a delegate.</summary>
        /// <typeparam name="TSource">The source type of the converter.</typeparam>
        /// <typeparam name="TDest">The destination type of the converter.</typeparam>
        /// <returns>The <see cref="Func{TSource, TDest}"/> that bound to the converter.</returns>
        public static Func<TSource, TDest> BindConverterAsFunc<TSource, TDest>() =>
            BindConverterUnchecked(typeof(TSource), typeof(TDest), typeof(Func<TSource, TDest>)) as Func<TSource, TDest>;

        /// <summary>Bind a converter to a delegate.</summary>
        /// <param name="srcType">The source type of the converter.</param>
        /// <param name="destType">The destination type of the converter.</param>
        /// <param name="delegateType">
        /// The type of the delegate.
        /// It will return <see cref="Func{TSource, TDest}"/> if it is null.
        /// </param>
        /// <returns>The delegate that bound to the converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="srcType"/> or <paramref name="destType"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="delegateType"/> is not a delegate type,
        /// or the delegate type is not compatible with the converter.
        /// </exception>
        public static Delegate BindConverter(Type srcType, Type destType, Type delegateType = null) {
            if (srcType == null) throw new ArgumentNullException(nameof(srcType));
            if (destType == null) throw new ArgumentNullException(nameof(destType));
            if (delegateType != null) {
                if (!delegateType.IsSubclassOf(typeof(Delegate))) throw new ArgumentException($"Type {delegateType} is not a delegate type");
                var invokeMethod = delegateType.GetMethod("Invoke");
                var parameters = invokeMethod.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != srcType || invokeMethod.ReturnType != destType)
                    throw new ArgumentException($"Delegate type {delegateType} is not compatible with converter from {srcType} to {destType}");
            }
            return BindConverterUnchecked(srcType, destType, delegateType);
        }

        static Delegate BindConverterUnchecked(Type srcType, Type destType, Type delegateType) {
            lock (converterCache) {
                if (!converterCache.TryGetValue((srcType, destType, delegateType), out var converterFunc)) {
                    var method =
                        destType.IsAssignableFrom(srcType) ?
                            typeof(Delegates).GetMethod(nameof(ToBaseType), STATIC_FLAGS).MakeGenericMethod(srcType, destType) :
                        srcType.IsAssignableFrom(destType) ?
                            typeof(Delegates).GetMethod(nameof(ToInheritedType), STATIC_FLAGS).MakeGenericMethod(srcType, destType) :
                        TypeInfo.Get(destType).TryGetConverter(srcType, true, out var m) ? m :
                        TypeInfo.Get(srcType).TryGetConverter(destType, false, out m) ? m :
                        srcType.IsPrimitive && destType.IsPrimitive ?
                            typeof(Convert).GetMethod($"To{Type.GetTypeCode(destType)}", STATIC_FLAGS, null, new[] { srcType }, null) :
                        typeof(Delegates).GetMethod(nameof(ToType), STATIC_FLAGS).MakeGenericMethod(srcType, destType);
                    if (method != null)
                        converterFunc = Delegate.CreateDelegate(
                            delegateType ?? typeof(Func<,>).MakeGenericType(srcType, destType),
                            method, false
                        );
                    converterCache.Add((srcType, destType, delegateType), converterFunc);
                }
                return converterFunc;
            }
        }

        static TDest ToBaseType<TSource, TDest>(TSource value) where TSource : TDest => value;

        static TDest ToInheritedType<TSource, TDest>(TSource value) where TDest : TSource => (TDest)value;

        static TDest ToType<TSource, TDest>(TSource value) => (TDest)Convert.ChangeType(value, typeof(TDest));
        #endregion

        #region Bind Operators
        /// <summary>Bind an unary operator to a delegate.</summary>
        /// <typeparam name="T">The operand type of the operator.</typeparam>
        /// <typeparam name="TResult">The result type of the operator.</typeparam>
        /// <param name="op">The operator type.</param>
        /// <returns>The delegate that bound to the operator.</returns>
        public static Func<T, TResult> BindOperator<T, TResult>(ExpressionType op) =>
            BindOperatorUnchecked(op, null, typeof(T), typeof(TResult), typeof(Func<T, TResult>)) as Func<T, TResult>;

        /// <summary>Bind a binary operator to a delegate.</summary>
        /// <typeparam name="TLeft">The left operand type of the operator.</typeparam>
        /// <typeparam name="TRight">The right operand type of the operator.</typeparam>
        /// <typeparam name="TResult">The result type of the operator.</typeparam>
        /// <param name="op">The operator type.</param>
        /// <returns>The delegate that bound to the operator.</returns>
        public static Func<TLeft, TRight, TResult> BindOperator<TLeft, TRight, TResult>(ExpressionType op) =>
            BindOperatorUnchecked(op, typeof(TLeft), typeof(TRight), typeof(TResult), typeof(Func<TLeft, TRight, TResult>)) as Func<TLeft, TRight, TResult>;

        /// <summary>Bind an operator to a delegate.</summary>
        /// <param name="op">The operator type.</param>
        /// <param name="lhs">The left operand type of the operator. Set to null if it is an unary operator.</param>
        /// <param name="rhs">The right operand type of the operator.</param>
        /// <param name="result">The result type of the operator.</param>
        /// <param name="delegateType">
        /// The type of the delegate.
        /// It will return <see cref="Func{T, TResult}"/> or <see cref="Func{TLeft, TRight, TResult}"/> (depends on operator type) if it is null.
        /// </param>
        /// <returns>The delegate that bound to the operator.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rhs"/> or <paramref name="result"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="op"/> is not an unary or binary operator,
        /// or the delegate type is not compatible with the operator.
        /// </exception>
        public static Delegate BindOperator(ExpressionType op, Type lhs, Type rhs, Type result, Type delegateType = null) {
            var parameterCount = op.GetOperatorParameterCount();
            switch (parameterCount) {
                case 1: if (rhs == null) rhs = lhs; break;
                case 2: if (lhs == null) throw new ArgumentNullException(nameof(lhs)); break;
                default: return null;
            }
            if (rhs == null) throw new ArgumentNullException(nameof(rhs));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (delegateType != null) {
                if (!delegateType.IsSubclassOf(typeof(Delegate))) throw new ArgumentException($"Type {delegateType} is not a delegate type");
                var invokeMethod = delegateType.GetMethod("Invoke");
                var parameters = invokeMethod.GetParameters();
                switch (parameterCount) {
                    case 1:
                        if (parameters.Length != 1 || parameters[0].ParameterType != rhs || invokeMethod.ReturnType != result)
                            throw new ArgumentException($"Delegate type {delegateType} is not compatible with operator {op} from {rhs} to {result}");
                        break;
                    case 2:
                        if (parameters.Length != 2 || parameters[0].ParameterType != lhs || parameters[1].ParameterType != rhs || invokeMethod.ReturnType != result)
                            throw new ArgumentException($"Delegate type {delegateType} is not compatible with operator {op} from {lhs} and {rhs} to {result}");
                        break;
                }
            }
            return BindOperatorUnchecked(op, lhs, rhs, result, delegateType);
        }

        static Delegate BindOperatorUnchecked(ExpressionType op, Type lhs, Type rhs, Type result, Type delegateType) {
            lock (operatorCache) {
                if (!operatorCache.TryGetValue((lhs, rhs, result, op, delegateType), out var operatorFunc)) {
                    var method = FindOperatorMethod(lhs, rhs, result, lhs, op) ?? FindOperatorMethod(rhs, lhs, result, rhs, op);
                    if (method != null)
                        operatorFunc = Delegate.CreateDelegate(
                            delegateType ?? (lhs == null ?
                                typeof(Func<,>).MakeGenericType(rhs, result) :
                                typeof(Func<,,>).MakeGenericType(lhs, rhs, result)
                            ),
                            method, false
                        );
                    operatorCache.Add((lhs, rhs, result, op, delegateType), operatorFunc);
                }
                return operatorFunc;
            }
        }

        static MethodInfo FindOperatorMethod(Type lhs, Type rhs, Type result, Type searchType, ExpressionType op) {
            if (searchType == null) return null;
            var opName = op.ToOperatorMethodName();
            if (string.IsNullOrEmpty(opName)) return null;
            var parameterCount = op.GetOperatorParameterCount();
            if (parameterCount < 2 && rhs == null) rhs = lhs;
            if (!TypeInfo.Get(searchType).TryGetMethods(opName, out var methods)) return null;
            foreach (var method in methods) {
                if (method.ReturnType != result) continue;
                var parameters = method.GetParameters();
                switch (parameterCount) {
                    case 1: if (parameters.Length != 1 || parameters[0].ParameterType != rhs) continue; break;
                    case 2: if (parameters.Length != 2 || parameters[0].ParameterType != lhs || parameters[1].ParameterType != rhs) continue; break;
                    default: continue;
                }
                return method;
            }
            return null;
        }
        #endregion
    }
}