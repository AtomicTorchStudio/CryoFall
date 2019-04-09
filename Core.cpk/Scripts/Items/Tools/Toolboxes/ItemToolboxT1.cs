namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    public class ItemToolboxT1 : ProtoItemToolToolbox
    {
        public override double ConstructionSpeedMultiplier => 1;

        public override string Description =>
            "A good toolbox is necessary to build any structure.";

        public override ushort DurabilityMax => 400;

        public override string Name => "Simple toolbox";
    }
}