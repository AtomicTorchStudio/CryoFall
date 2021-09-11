namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseRateWithRange<TServerRate, TValue>
        : BaseRate<TServerRate, TValue>, IRateWithRange<TValue>
        where TValue : struct, IEquatable<TValue>, IComparable<TValue>
        where TServerRate : BaseRateWithRange<TServerRate, TValue>, new()
    {
        public abstract TValue ValueMax { get; }

        public virtual TValue ValueMaxReasonable => this.ValueMax;

        public abstract TValue ValueMin { get; }

        public virtual TValue ValueMinReasonable => this.ValueMin;

        public abstract TValue ValueStepChange { get; }

        public abstract RateValueType ValueType { get; }

        protected static string SharedGetAllowedRangeString(string min, string max)
        {
            return string.Format("Allowed range: from {0} to {1}",
                                 min,
                                 max);
        }

        protected sealed override TValue ServerReadValue()
        {
            Api.Assert(this.ValueMax.CompareTo(this.ValueMin) > 0,
                       "Max value must be larger than the min value");
            return this.ServerReadValueWithRange();
        }

        protected abstract TValue ServerReadValueWithRange();
    }
}