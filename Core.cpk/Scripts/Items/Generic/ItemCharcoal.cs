namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCharcoal : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description =>
            "Charcoal is left as a byproduct of burning wood. Can be used itself as fuel.";

        public double FuelAmount => 15;

        public override string Name => "Charcoal";
    }
}