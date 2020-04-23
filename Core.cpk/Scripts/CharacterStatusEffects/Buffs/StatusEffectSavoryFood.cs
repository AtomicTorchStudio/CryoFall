namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectSavoryFood : ProtoStatusEffect
    {
        public override string Description =>
            "You've recently eaten a savory meal. This makes your life seem more gratifying.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Savory food";

        protected override void PrepareEffects(Effects effects)
        {
            // small bonuses to learning
            effects.AddPercent(this, StatName.LearningsPointsGainMultiplier, 10)
                   .AddPercent(this, StatName.SkillsExperienceGainMultiplier, 10);
        }
    }
}