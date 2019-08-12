namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemPaper : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description => "Paper is a versatile material with many uses and can be easily prepared from wood.";

        public double FuelAmount => 10;

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Paper";
    }
}