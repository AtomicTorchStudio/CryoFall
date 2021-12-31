namespace AtomicTorch.CBND.CoreMod.Items.Tools.Lights
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemOilLamp : ProtoItemToolLight
    {
        public override string Description =>
            "Portable light source in a convenient package! Can be quickly refilled with camp fuel.";

        public override uint DurabilityMax => 7000;

        public override string Name => "Oil lamp";

        protected override BaseClientComponentLightSource ClientCreateLightSource(
            IItem item,
            ICharacter character)
        {
            var lightSource = base.ClientCreateLightSource(item, character);

            // add light flickering
            character.ClientSceneObject
                     .AddComponent<ClientComponentLightSourceEffectFlickering>()
                     .Setup(lightSource,
                            flickeringPercents: 5,
                            flickeringChangePercentsPerSecond: 33);

            return lightSource;
        }

        protected override void PrepareProtoItemLight(ItemLightConfig lightConfig, ItemFuelConfig fuelConfig)
        {
            lightConfig.Color = LightColors.OilLamp;
            lightConfig.ScreenOffset = (42, 60);
            lightConfig.Size = 18;

            fuelConfig.FuelCapacity = 750; // about 12.5 minutes
            fuelConfig.FuelAmountInitial = 0;
            fuelConfig.FuelUsePerSecond = 1;
            fuelConfig.FuelProtoItemsList.AddAll<IProtoItemFuelOil>();
        }
    }
}