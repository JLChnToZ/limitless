using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Special dynamic object that can be used to access events.</summary>
    /// <remarks>This should not be directly used. Use <see cref="Limitless"/> instead.</remarks>
    public class LimitlessEvent: LimitlessInvokable {
        readonly EventInfo eventInfo;
        readonly Lazy<MethodInfo> addMethod, removeMethod;
        Delegate backingDelegate;

        protected override MethodInfo[] MethodInfos {
             get {
                methodInfos[0] = eventInfo.GetUndelyRaiseMethod(out backingDelegate, target);
                if (methodInfos[0] == null)
                    throw new InvalidOperationException("Raise method unavailable for this context.");
                return methodInfos;
             }
        }

        protected override object InvokeTarget => backingDelegate ?? target;

        internal LimitlessEvent(object target, EventInfo eventInfo) : base(target, new MethodInfo[1]) {
            this.eventInfo = eventInfo;
            addMethod = new Lazy<MethodInfo>(GetAddMethod);
            removeMethod = new Lazy<MethodInfo>(GetRemoveMethod);
        }

        MethodInfo GetAddMethod() => eventInfo.GetAddMethod(true);

        MethodInfo GetRemoveMethod() => eventInfo.GetRemoveMethod(true);

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            arg = InternalUnwrap(arg);
            var eventType = eventInfo.EventHandlerType;
            Delegate targetDelegate = null;
            if (arg is Delegate argDelegate) {
                var delegateType = argDelegate.GetType();
                if (delegateType == eventType || delegateType.IsSubclassOf(eventType))
                    targetDelegate = argDelegate;
                else {
                    // Rewrap delegate to avoid type mismatch
                    targetDelegate = argDelegate.ConvertAndFlatten(eventType);
                    if (targetDelegate == null) {
                        result = null;
                        return false;
                    }
                }
            } else if (arg is LimitlessInvokable invokable && !invokable.TryCreateDelegate(eventType, out targetDelegate)) {
                result = null;
                return false;
            }
            // Null is allowed, it will be ignored
            switch (binder.Operation) {
                case ExpressionType.AddAssign: {
                    var addMethod = this.addMethod.Value;
                    if (addMethod != null) {
                        addMethod.Invoke(target, new [] { targetDelegate });
                        result = this;
                        return true;
                    }
                    break;
                }
                case ExpressionType.SubtractAssign: {
                    var removeMethod = this.removeMethod.Value;
                    if (removeMethod != null) {
                        removeMethod.Invoke(target, new [] { targetDelegate });
                        result = this;
                        return true;
                    }
                    break;
                }
            }
            result = null;
            return false;
        }
    }
}