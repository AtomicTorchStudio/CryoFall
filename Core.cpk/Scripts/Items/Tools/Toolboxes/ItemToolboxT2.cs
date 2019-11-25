namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemToolboxT2 : ProtoItemToolToolbox
    {
        public override double ConstructionSpeedMultiplier => 1.5;

        public override string Description => GetProtoEntity<ItemToolboxT1>().Description;

        public override uint DurabilityMax => 800;

        public override string Name => "Iron toolbox";

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetToolbox()
        {
            // TODO: use T2 sound
            return ObjectsSoundsPresets.ObjectConstructionSite.Clone()
                                       .Replace(ObjectSound.InteractProcess, "Items/Tools/ToolboxT2/UseProcess");
        }
    }
}