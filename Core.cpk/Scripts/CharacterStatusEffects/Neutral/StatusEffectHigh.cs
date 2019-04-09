namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Stats;

    /// <summary>
    /// Note: it also works the same as painkiller but reduces pain by half (calculated inside the pain effect)
    /// 
    /// Also, don't do drugs in reality. It destroys your life. We condemn use of any drugs or other mind altering substances.
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

            // -50% energy consumption
            effects.AddPercent(this, StatName.RunningStaminaConsumptionPerSecond, -50);

            // movement speed +10%
            effects.AddPercent(this, StatName.MoveSpeed, 10);

            // increase combat effectiveness
            effects.AddPercent(this, StatName.WeaponMeleeDamageBonusMultiplier, 10);
        }
    }
}