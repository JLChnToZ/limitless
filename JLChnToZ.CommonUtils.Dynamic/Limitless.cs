using System;
using System.Collections;
using System.Dynamic;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>
    /// The dynamic access pass to any methods, properties and fields of any types and instances, whatever it is public or private, static or non-static.
    /// </summary>
    /// <remarks>
    /// You can use the static methods from this class as a starting point.
    /// To initiate the dynamic instance with specified types, you can use <see cref="Type"/> object, type name strings or other wrapped dynamic objects.
    /// </remarks>
    /// <example><code><![CDATA[
    ///     // To construct an instance and call a method
    ///     var newString = Limitless.Construct(typeof(StringBuilder), "Hello, ").Append("World!").ToString();
    ///     // To call a static generic method, you may use .of(...) to specify generic type arguments.
    ///     Limitless.Static("UnityEngine.Object, UnityEngine").FindObjectOfType.of(typeof(Rigidbody))();
    ///     // To wrap an existing object.
    ///     var obj = new MyObject();
    ///     var objWrapped = Limitless.Wrap(obj);
    ///     // To re-wrap an object narrowed to a type, useful for accessing hidden interface members
    ///     var narrowedType = Lmitless.Wrap(obj, typeof(IMyInterface));
    ///     var narrowedType2 = Lmitless.Wrap(objWrapped, typeof(IMyInterface));
    ///     // To wrap a method to a delegate
    ///     Func<double, double> absWrapped = Limitless.Static(typeof(Math)).Abs;
    /// ]]></code></example>
    public class Limitless: DynamicObject, IEnumerable {
        protected readonly internal object target;
        protected readonly internal Type type;
        protected readonly internal TypeInfo typeInfo;

        /// <summary>Get access to all static members of a type.</summary>
        /// <param name="type">The type to access.</param>
        /// <returns>The dynamic object with access to all static members of the type.</returns>
        public static dynamic Static(string typeName) => Static(Type.GetType(typeName, true));

        /// <summary>Get access to all static members of a type.</summary>
        /// <param name="type">The type to access.</param>
        /// <returns>The dynamic object with access to all static members of the type.</returns>
        public static dynamic Static(Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return new Limitless(null, type);
        }

        /// <summary>Get access to all static members of a type.</summary>
        /// <param name="source">The dynamic object of the type to access.</param>
        /// <returns>The dynamic object with access to all static members of the type.</returns>
        public static dynamic Static(Limitless source) => Static(source.type);

        /// <summary>Wrap an object to dynamic object.</summary>
        /// <param name="obj">The object to wrap.</param>
        /// <param name="type">Optional base/interface type to wrap.</param>
        /// <returns>The dynamic object with access to all members of it.</returns>
        public static dynamic Wrap(object obj, Type type = null) {
            if (obj == null) return null;
            if (obj is Limitless ? type == null : obj is DynamicObject) return obj;
            if (type == null) return InternalWrap(obj);
            obj = InternalUnwrap(obj);
            var objType = obj.GetType();
            if (!type.IsAssignableFrom(objType) && !objType.IsAssignableFrom(type))
                throw new ArgumentException("Type mismatch", nameof(type));
            return new Limitless(obj, type);
        }

        /// <summary>Wrap an object to dynamic object.</summary>
        /// <param name="obj">The object to wrap.</param>
        /// <param name="typeName">The name of base/interface type to wrap.</param>
        /// <returns>The dynamic object with access to all members of it.</returns>
        public static dynamic Wrap(object obj, string typeName) => Wrap(obj, Type.GetType(typeName, true));

        /// <summary>Construct an object with specified type and arguments.</summary>
        /// <param name="type">The type of object to construct.</param>
        /// <param name="args">The arguments to construct the object.</param>
        /// <returns>The dynamic instance with access to all members of it.</returns>
        public static dynamic Construct(Type type, params object[] args) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (TypeInfo.Get(type).TryConstruct(args, out var result)) return result;
            throw new MissingMethodException("No constructor matched");
        }

        /// <summary>Construct an object with specified type and arguments.</summary>
        /// <param name="typeName">The name of type of object to construct.</param>
        /// <param name="args">The arguments to construct the object.</param>
        /// <returns>The dynamic instance with access to all members of it.</returns>
        public static dynamic Construct(string typeName, params object[] args) => Construct(Type.GetType(typeName, true), args);

        /// <summary>Construct an object with specified type and arguments.</summary>
        /// <param name="source">The dynamic object of the type to construct.</param>
        /// <param name="args">The arguments to construct the object.</param>
        /// <returns>The dynamic instance with access to all members of it.</returns>
        public static dynamic Construct(Limitless source, params object[] args) => Construct(source.type, args);

        protected internal Limitless(object target = null, Type type = null) {
            this.target = target;
            this.type = type ?? target?.GetType() ?? typeof(object);
            typeInfo = TypeInfo.Get(this.type);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (typeInfo.TryGetValue(target, binder.Name, out result))
                return true;
            if (typeInfo.TryGetMethods(binder.Name, out var methods)) {
                result = new LimitlessInvokable(target, methods);
                return true;
            }
            if (typeInfo.TryGetSubType(binder.Name, out var subType)) {
                result = Static(subType);
                return true;
            }
            return typeInfo.TryGetValue(target, new[] { binder.Name }, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) =>
            typeInfo.TrySetValue(target, binder.Name, value) ||
            typeInfo.TrySetValue(target, new[] { binder.Name }, value);

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) =>
            typeInfo.TryGetValue(target, indexes, out result);

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) =>
            typeInfo.TrySetValue(target, indexes, value);

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) =>
            typeInfo.TryInvoke(target, binder.Name, args, out result);

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            if (target is Delegate del) {
                result = InternalWrap(del.DynamicInvoke(InternalUnwrap(args)));
                InternalWrap(args, args);
                return true;
            }
            result = null;
            return false;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) =>
            typeInfo.TryInvoke(target, $"op_{binder.Operation}", new[] { arg }, out result);

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) =>
            typeInfo.TryInvoke(target, $"op_{binder.Operation}", null, out result);

        public override bool TryConvert(ConvertBinder binder, out object result) {
            if (typeInfo.TryCast(target, binder.Type, out result))
                return true;
            try {
                if (target is IConvertible convertible) {
                    result = convertible.ToType(binder.Type, null);
                    return true;
                }
            } catch (Exception) { }
            result = null;
            return false;
        }

        // Supports await ... syntax
        public virtual LimitlessAwaiter GetAwaiter() =>
            new LimitlessAwaiter(typeInfo.TryInvoke(target, nameof(GetAwaiter), null, out var awaiter) ? InternalUnwrap(awaiter) : target);

        // Supports foreach (... in ...) { ... } syntax
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual IEnumerator GetEnumerator() {
            if (target is IEnumerable enumerable) return new LimitlessEnumerator(enumerable.GetEnumerator());
            throw new InvalidOperationException("The object is not enumerable.");
        }

        public new Type GetType() => target?.GetType() ?? type;

        public override string ToString() => target?.ToString() ?? type.ToString();

        public override int GetHashCode() => target?.GetHashCode() ?? 0;
    }

}
