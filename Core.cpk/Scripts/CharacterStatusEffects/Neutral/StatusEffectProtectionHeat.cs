namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    /// <summary>
    /// Note: This status effect has no logic by itself. Instead the effect that adds
    /// heat influence does the checking to see if the amount applied should be reduced
    /// based on this (based on whether this effect is present or not).
    /// </summary>
    public class StatusEffectProtectionHeat : ProtoStatusEffect
    {
        public override string Description =>
            "You are covered in a heat-resistant gel, significantly reducing any thermal damage you take from environmental sources.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Heat protection";
    }
}