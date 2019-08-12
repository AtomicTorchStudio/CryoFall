namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoItemToolWateringCan
        : IProtoItemTool,
          IProtoItemWithCharacterAppearance,
          IProtoItemWithHotbarOverlay
    {
        double ActionDurationWateringSeconds { get; }

        byte WaterCapacity { get; }

        TimeSpan WateringDuration { get; }

        bool SharedCanWater(IItem itemWateringCan);

        byte SharedGetWaterAmount(IItem itemWateringCan);

        void SharedOnRefilled(IItem itemWateringCan, byte currentWaterAmount);

        void SharedOnWatered(IItem itemWateringCan, IStaticWorldObject staticWorldObject);
    }
}