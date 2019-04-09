namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemLiquidStorage : IProtoItem
    {
        ushort Capacity { get; }

        LiquidType LiquidType { get; }
    }
}