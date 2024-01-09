using System;
using System.Runtime.CompilerServices;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Special dynamic object that can be used to await for async methods.</summary>
    /// <remarks>This should not be directly used. Use <see cref="Limitless"/> instead.</remarks>
    public class LimitlessAwaiter: Limitless, INotifyCompletion {
        internal LimitlessAwaiter(object target) : base(target, null) { }

        void INotifyCompletion.OnCompleted(Action continuation) {
            if (target is INotifyCompletion notifyCompletion)
                notifyCompletion.OnCompleted(continuation);
            else
                continuation();
        }

        public dynamic GetResult() {
            if (target is INotifyCompletion && typeInfo.TryInvoke(target, nameof(GetResult), null, out var result))
                return result;
            return InternalWrap(target);
        }

        public override LimitlessAwaiter GetAwaiter() => this;
    }
}
