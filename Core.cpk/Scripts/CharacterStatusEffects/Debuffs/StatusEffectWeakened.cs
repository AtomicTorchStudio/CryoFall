namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectWeakened : ProtoStatusEffect
    {
        public override string Description =>
            "You feel rather weak. Undertaking any tasks seems much more difficult. Give it some time to restore your condition and avoid dangerous situations.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Weakened";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.MiningSpeed, -50);
            effects.AddPercent(this, StatName.WoodcuttingSpeed, -50);
            effects.AddPercent(this, StatName.BuildingSpeed, -50);
            effects.AddPercent(this, StatName.CraftingSpeed, -50);
            effects.AddPercent(this, StatName.FarmingTasksSpeed, -50);
            effects.AddPercent(this, StatName.ForagingSpeed, -50);
            effects.AddPercent(this, StatName.HuntingLootingSpeed, -50);
            effects.AddPercent(this, StatName.SearchingSpeed, -50);
            effects.AddPercent(this, StatName.ItemExplosivePlantingSpeedMultiplier, -50); // slower
        }
    }
}