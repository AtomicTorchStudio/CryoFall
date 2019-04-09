namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class ItemBottleWater : ProtoItemFood, IProtoItemLiquidStorage
    {
        public ushort Capacity => 10;

        public override string Description =>
            "Could be consumed to replenish water. Also useful in a multitude of crafting recipes.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public LiquidType LiquidType => LiquidType.Water;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Bottle with pure water";

        public override ushort OrganicValue => 0;

        public override float StaminaRestore => 20;

        public override float WaterRestore => 30;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            ItemBottleEmpty.ServerSpawnEmptyBottle(data.Character);
            base.ServerOnEat(data);
        }
    }
}