namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemToolboxT3 : ProtoItemToolToolbox
    {
        public override double ConstructionSpeedMultiplier => 2.0f;

        public override string Description => GetProtoEntity<ItemToolboxT1>().Description;

        public override uint DurabilityMax => 1200;

        public override string Name => "Steel toolbox";

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetToolbox()
        {
            // TODO: use T3 sound
            return ObjectsSoundsPresets.ObjectConstructionSite.Clone()
                                       .Replace(ObjectSound.InteractProcess, "Items/Tools/ToolboxT3/UseProcess");
        }
    }
}