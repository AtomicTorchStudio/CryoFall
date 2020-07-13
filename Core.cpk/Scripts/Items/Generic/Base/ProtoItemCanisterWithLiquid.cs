namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public abstract class ProtoItemCanisterWithLiquid : ProtoItemGeneric, IProtoItemLiquidStorage
    {
        public virtual ushort Capacity => 10;

        public abstract LiquidType LiquidType { get; }

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;
    }
}