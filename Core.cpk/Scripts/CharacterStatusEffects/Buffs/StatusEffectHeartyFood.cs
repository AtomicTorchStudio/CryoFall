namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectHeartyFood : ProtoStatusEffect
    {
        public override string Description =>
            "You've recently eaten a hearty meal. Any tasks you undertake will feel easier.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Hearty food";

        protected override void PrepareEffects(Effects effects)
        {
            // small bonuses in many areas
            effects.AddPercent(this, StatName.MiningSpeed,         10);
            effects.AddPercent(this, StatName.WoodcuttingSpeed,    10);
            effects.AddPercent(this, StatName.BuildingSpeed,       10);
            effects.AddPercent(this, StatName.CraftingSpeed,       10);
            effects.AddPercent(this, StatName.FarmingTasksSpeed,   10);
            effects.AddPercent(this, StatName.ForagingSpeed,       10);
            effects.AddPercent(this, StatName.HuntingLootingSpeed, 10);
            effects.AddPercent(this, StatName.SearchingSpeed,      10);
        }
    }
}