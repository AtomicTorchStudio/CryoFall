namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Stats;

    /// <summary>
    /// Don't do drugs in reality. It destroys your life. We condemn use of any drugs or other mind altering substances.
    /// </summary>
    public class StatusEffectHigh : ProtoStatusEffect
    {
        public override string Description
            => "You are high on something. It makes you feel rather happy, for some reason...";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "High";

        protected override void PrepareEffects(Effects effects)
        {
            // energy regeneration -50%
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond, -50);

            // movement speed +10%
            effects.AddPercent(this, StatName.MoveSpeed, 10);

            // increase combat effectiveness
            effects.AddPercent(this, StatName.WeaponMeleeDamageBonusMultiplier, 10);

            // pain increase -50%
            effects.AddPercent(this, StatName.PainIncreaseRateMultiplier, -50);
        }
    }
}