namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemPlanks : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description => "Mostly used as construction materials. Can also be burned as fuel.";

        public double FuelAmount => 10;

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Wood planks";
    }
}