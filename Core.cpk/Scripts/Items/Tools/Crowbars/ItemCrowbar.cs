namespace AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCrowbar : ProtoItemToolCrowbar
    {
        public override double DeconstructionSpeedMultiplier => 2.0;

        public override string Description =>
            "Useful for quickly deconstructing buildings inside of your land claim area.";

        public override uint DurabilityMax => 750;

        public override string Name => "Crowbar";

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetCrowbar()
        {
            // no process sound
            return ObjectsSoundsPresets.ObjectConstructionSite.Clone();
        }
    }
}