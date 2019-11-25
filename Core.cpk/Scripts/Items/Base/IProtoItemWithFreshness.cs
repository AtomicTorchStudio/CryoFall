namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;

    public interface IProtoItemWithFreshness : IProtoItemWithSlotOverlay
    {
        TimeSpan FreshnessDuration { get; }

        uint FreshnessMaxValue { get; }
    }
}