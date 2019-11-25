namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemEnergyTablets : ProtoItemMedical
    {
        public override string Description =>
            "Simple medicine to quickly recover fatigue.";

        public override float FoodRestore => 1;

        public override double MedicalToxicity => 0.05;

        public override string Name => "Energy tablets";

        public override float StaminaRestore => 200;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalTablets;
        }
    }
}