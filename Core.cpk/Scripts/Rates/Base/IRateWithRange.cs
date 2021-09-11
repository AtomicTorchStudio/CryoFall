namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;

    public interface IRateWithRange<out TValue> : IRate<TValue>
        where TValue : IComparable<TValue>
    {
        TValue ValueMax { get; }

        TValue ValueMaxReasonable { get; }

        TValue ValueMin { get; }

        TValue ValueMinReasonable { get; }

        TValue ValueStepChange { get; }

        RateValueType ValueType { get; }
    }
}