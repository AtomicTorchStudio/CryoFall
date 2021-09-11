namespace AtomicTorch.CBND.CoreMod.Items.Tools.Axes
{
    public class ItemAxeSteel : ProtoItemToolAxe
    {
        public override double DamageApplyDelay => 0.0625;

        public override double DamageToNonTree => 20;

        public override double DamageToTree => 70;

        public override string Description
            => "Steel axe is ideal for chopping trees. Faster and more durable than iron axe.";

        public override uint DurabilityMax => 1400;

        public override double FireAnimationDuration => 0.5;

        public override string Name => "Steel axe";
    }
}