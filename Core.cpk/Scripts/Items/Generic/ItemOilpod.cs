namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemOilpod : ProtoItemGeneric, IProtoItemFuelSolid, IProtoItemOrganic
    {
        public override string Description =>
            "Inedible fruit of a special genetically engineered plant. Contains high concentration of petroleum products and derivatives.";

        public double FuelAmount => 25;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Oilpod fruit";

        public ushort OrganicValue => 3;
    }
}