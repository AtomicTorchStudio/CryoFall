namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemToolboxT1 : ProtoItemToolToolbox
    {
        public override double ConstructionSpeedMultiplier => 1;

        public override string Description =>
            "Toolbox is necessary to build or repair any of your structures. Make sure it's equipped and selected.";

        public override uint DurabilityMax => 400;

        public override string Name => "Simple toolbox";

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetToolbox()
        {
            return ObjectsSoundsPresets.ObjectConstructionSite.Clone()
                                       .Replace(ObjectSound.InteractProcess, "Items/Tools/ToolboxT1/UseProcess");
        }
    }
}