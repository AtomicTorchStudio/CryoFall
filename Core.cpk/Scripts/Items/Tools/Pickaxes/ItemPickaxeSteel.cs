namespace AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes
{
    public class ItemPickaxeSteel : ProtoItemToolPickaxe
    {
        public override double DamageApplyDelay => 0.0625;

        public override double DamageToMinerals => 90;

        public override double DamageToNonMinerals => 20;

        public override string Description =>
            "Steel pickaxe can be used to mine mineral deposits. Faster and more durable than iron pickaxe.";

        public override uint DurabilityMax => 1800;

        public override double FireAnimationDuration => 0.5;

        public override string Name => "Steel pickaxe";
    }
}