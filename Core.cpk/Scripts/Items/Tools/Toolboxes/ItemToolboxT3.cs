namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    public class ItemToolboxT3 : ProtoItemToolToolbox
    {
        public override double ConstructionSpeedMultiplier => 2.0f;

        public override string Description =>
            "A good toolbox is necessary to build any structure.";

        public override ushort DurabilityMax => 1200;

        public override string Name => "Steel toolbox";
    }
}