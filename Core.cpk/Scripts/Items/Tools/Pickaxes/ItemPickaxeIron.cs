namespace AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes
{
    public class ItemPickaxeIron : ProtoItemToolPickaxe
    {
        public override double DamageToMinerals => 65;

        public override double DamageToNonMinerals => 16;

        public override string Description =>
            "Iron pickaxe can be used to mine mineral deposits. Faster and more durable than stone pickaxe.";

        public override uint DurabilityMax => 1200;

        public override string Name => "Iron pickaxe";
    }
}