namespace AtomicTorch.CBND.CoreMod.Items.Tools.Lights
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI;

    public class ItemFlashlight : ProtoItemToolLight
    {
        public override string Description =>
            "Nothing beats a powerful flashlight. This model produces upwards of 3,000 lumens while using simple disposable batteries.";

        public override ushort DurabilityMax => 10000;

        public override string Name => "Flashlight";

        protected override void PrepareProtoItemLight(ItemLightConfig lightConfig, ItemFuelConfig fuelConfig)
        {
            lightConfig.Color = LightColors.Flashlight;
            lightConfig.ScreenOffset = (3, -1);
            lightConfig.Size = 17;

            fuelConfig.FuelTitle = CoreStrings.TitleEnergyCharge;
            fuelConfig.FuelCapacity = 500; // about 8.5 minutes
            fuelConfig.FuelAmountInitial = 0;
            fuelConfig.FuelUsePerSecond = 1;
            fuelConfig.FuelProtoItemsList.AddAll<IProtoItemFuelElectricity>();
        }
    }
}