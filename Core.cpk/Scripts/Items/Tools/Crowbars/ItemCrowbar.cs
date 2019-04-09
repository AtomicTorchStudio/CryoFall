namespace AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars
{
    public class ItemCrowbar : ProtoItemToolCrowbar
    {
        public override double DeconstructionSpeedMultiplier => 1.5f;

        public override string Description =>
            "Useful for quickly deconstructing buildings inside of your land claim area.";

        public override ushort DurabilityMax => 750;

        public override string Name => "Crowbar";
    }
}