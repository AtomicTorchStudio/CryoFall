namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemRope : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description =>
            "Just normal rope. Can be used in a variety of ways. Excess rope can be burned.";

        public double FuelAmount => 10;

        public override string Name => "Rope";
    }
}