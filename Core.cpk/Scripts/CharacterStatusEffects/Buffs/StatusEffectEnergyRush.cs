namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectEnergyRush : ProtoStatusEffect
    {
        public override string Description =>
            "You have used some stimulants and your stamina regeneration is much faster.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Energy rush";

        protected override void PrepareEffects(Effects effects)
        {
            // +100% to energy regeneration
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond, 100);

            // -50% energy consumption
            effects.AddPercent(this, StatName.RunningStaminaConsumptionPerSecond, -50);

            // increase maximum energy by 50
            effects.AddValue(this, StatName.StaminaMax, 50);
        }
    }
}