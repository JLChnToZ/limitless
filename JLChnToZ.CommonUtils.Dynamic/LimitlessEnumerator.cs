using System.Collections;

namespace JLChnToZ.CommonUtils.Dynamic {
    using static Utilites;

    /// <summary>Special dynamic object that can be used to enumerate.</summary>
    /// <remarks>This should not be directly used. Use <see cref="Limitless"/> instead.</remarks>
    public class LimitlessEnumerator: Limitless, IEnumerator {
        internal LimitlessEnumerator(IEnumerator target) : base(target, null) { }

        public dynamic Current => InternalWrap(((IEnumerator)target).Current);

        public bool MoveNext() => ((IEnumerator)target).MoveNext();

        public void Reset() => ((IEnumerator)target).Reset();

        protected override IEnumerator GetEnumerator() => this;
    }
}
