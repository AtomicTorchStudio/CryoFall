namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectWeakened : ProtoStatusEffect
    {
        public override string Description =>
            "You feel rather weak. It's probably a good idea to avoid being exposed to harmful situations, such as radiation.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Weakened";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.RadiationPoisoningDamageMultiplier, 1000);

            effects.AddPercent(this, StatName.RadiationPoisoningAccumulationMultiplier, 10000);
        }
    }
}