namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFirelog : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description =>
            "Specially manufactured artificial logs. Burn many times longer than natural logs.";

        public double FuelAmount => 600;

        public override string Name => "Firelog";
    }
}