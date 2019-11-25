namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Braided
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemBraidedHelmetLamp : ProtoItemEquipmentHeadWithLight
    {
        public override string Description => GetProtoEntity<ItemBraidedChestplate>().Description;

        public override uint DurabilityMax => 500;

        public override bool IsHairVisible => false;

        public override string Name => "Makeshift lamp helmet";

        protected override BaseClientComponentLightSource ClientCreateLightSource(
            IItem item,
            ICharacter character,
            IClientSceneObject sceneObject)
        {
            var lightSource = base.ClientCreateLightSource(item, character, sceneObject);

            // add light flickering
            sceneObject.AddComponent<ClientComponentLightSourceEffectFlickering>()
                       .Setup(lightSource,
                              flickeringPercents: 5,
                              flickeringChangePercentsPerSecond: 33);

            return lightSource;
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.30,
                heat: 0.20,
                cold: 0.20,
                chemical: 0.10,
                electrical: 0.15,
                radiation: 0.15,
                psi: 0);
        }

        protected override void PrepareProtoItemEquipmentHeadWithLight(
            ItemLightConfig lightConfig,
            ItemFuelConfig fuelConfig)
        {
            lightConfig.Color = LightColors.OilLamp;
            lightConfig.ScreenOffset = (15, 2);
            lightConfig.Size = 16;

            fuelConfig.FuelCapacity = 500; // about 8.5 minutes
            fuelConfig.FuelAmountInitial = 0;
            fuelConfig.FuelUsePerSecond = 1;
            fuelConfig.FuelProtoItemsList.AddAll<IProtoItemFuelOil>();
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric.Clone()
                                    .Replace(ItemSound.Use,          "Items/Equipment/UseLight")
                                    .Replace(ItemSound.CannotSelect, "Items/Equipment/UseLight");
        }
    }
}