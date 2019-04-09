namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public interface IProtoItemFood : IProtoItemOrganic, IProtoItemWithSlotOverlay, IProtoItemUsableFromContainer
    {
        float FoodRestore { get; }

        TimeSpan FreshnessDuration { get; }

        uint FreshnessMaxValue { get; }

        float HealthRestore { get; }

        float StaminaRestore { get; }

        float WaterRestore { get; }
    }
}