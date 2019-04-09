namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    public class ItemToolboxT2 : ProtoItemToolToolbox
    {
        public override double ConstructionSpeedMultiplier => 1.5f;

        public override string Description =>
            "A good toolbox is necessary to build any structure.";

        public override ushort DurabilityMax => 800;

        public override string Name => "Iron toolbox";
    }
}