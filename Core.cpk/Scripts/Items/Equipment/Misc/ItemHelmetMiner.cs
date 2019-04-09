namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemHelmetMiner : ProtoItemEquipmentHeadWithLight
    {
        public override string Description =>
            "Convenient helmet that also provides a portable light source and keeps your hands free. Requires disposable batteries.";

        public override ushort DurabilityMax => 800;

        public override bool IsHairVisible => false;

        public override string Name => "Miner helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.40,
                heat: 0.25,
                cold: 0.15,
                chemical: 0.25,
                electrical: 0.20,
                radiation: 0.20,
                psi: 0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.30 / defense.Multiplier;
        }

        protected override void PrepareProtoItemEquipmentHeadWithLight(
            ItemLightConfig lightConfig,
            ItemFuelConfig fuelConfig)
        {
            lightConfig.Color = LightColors.OilLamp;
            lightConfig.ScreenOffset = (20, -3);
            lightConfig.Size = 13;

            fuelConfig.FuelCapacity = 500; // about 8.5 minutes
            fuelConfig.FuelAmountInitial = 0;
            fuelConfig.FuelUsePerSecond = 1;
            fuelConfig.FuelProtoItemsList.AddAll<IProtoItemFuelElectricity>();
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric.Clone()
                                    .Replace(ItemSound.Use, "Items/Equipment/UseLight");
        }
    }
}