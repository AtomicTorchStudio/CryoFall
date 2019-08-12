namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFertilizer : ProtoItemFertilizer
    {
        public const string ShortDescriptionText =
            @"Improves growth speed by {0}%
              [br]Improves yield";

        public static readonly DropItemConditionDelegate ConditionExtraYield
            = context => context.StaticWorldObject.GetPrivateState<PlantPrivateState>()
                                .AppliedFertilizerProto is ItemFertilizer;

        public ItemFertilizer()
        {
            this.FertilizerShortDescription = string.Format(ShortDescriptionText,
                                                            "+"
                                                            + (int)Math.Round(
                                                                (this.PlantGrowthSpeedMultiplier - 1.0) * 100,
                                                                MidpointRounding.AwayFromZero));
        }

        public override string Description =>
            "Significantly improves plant growth speed and increases amount harvested from a single plant.";

        public override string FertilizerShortDescription { get; }

        public override string Name => "Fertilizer";

        public override double PlantGrowthSpeedMultiplier => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFertilizer.Clone();
        }
    }
}