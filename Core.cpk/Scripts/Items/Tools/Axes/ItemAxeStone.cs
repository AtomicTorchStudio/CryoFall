namespace AtomicTorch.CBND.CoreMod.Items.Tools.Axes
{
    public class ItemAxeStone : ProtoItemToolAxe
    {
        public override double DamageToNonTree => 12;

        public override double DamageToTree => 45;

        public override string Description
            => "Stone axe can be used to chop trees.";

        // high penalty when hitting buildings such as a claimed wall/door
        public override double DurabilityDecreaseMultiplierWhenHittingBuildings => 30;

        public override uint DurabilityMax => 500;

        public override string Name => "Stone axe";
    }
}