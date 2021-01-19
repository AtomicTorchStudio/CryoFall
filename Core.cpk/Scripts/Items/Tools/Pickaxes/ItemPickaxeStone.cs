namespace AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes
{
    public class ItemPickaxeStone : ProtoItemToolPickaxe
    {
        public override double DamageApplyDelay => 0.075;

        public override double DamageToMinerals => 55;

        public override double DamageToNonMinerals => 12;

        public override string Description => "Stone pickaxe can be used to mine mineral deposits.";

        // high penalty when hitting buildings such as a claimed wall/door
        public override double DurabilityDecreaseMultiplierWhenHittingBuildings
            => base.DurabilityDecreaseMultiplierWhenHittingBuildings * 3;

        public override uint DurabilityMax => 600;

        public override double FireAnimationDuration => 0.6;

        public override string Name => "Stone pickaxe";
    }
}