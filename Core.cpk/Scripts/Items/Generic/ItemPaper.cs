namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemPaper : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description => "Just plain ol' paper.";

        public double FuelAmount => 10;

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Paper";
    }
}