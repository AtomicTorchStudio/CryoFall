namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCoal : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description => "Coal mined from the ground or from coal deposits. Works great as fuel.";

        public double FuelAmount => 100;

        public override string Name => "Coal";
    }
}