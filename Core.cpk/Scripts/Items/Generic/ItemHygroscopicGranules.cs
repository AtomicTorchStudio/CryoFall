namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemHygroscopicGranules : ProtoItemPlantWatering
    {
        public override string Description =>
            "Special highly hygroscopic granules that can be used to supplement soil. These granules pull moisture from the air, eliminating the need to water the plant.";

        public override string Name => "Hygroscopic granules";

        // permanent watering
        public override double WateringDuration => double.MaxValue;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFertilizer.Clone();
        }
    }
}