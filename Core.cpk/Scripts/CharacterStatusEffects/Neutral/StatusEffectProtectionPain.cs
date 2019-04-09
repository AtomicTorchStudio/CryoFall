namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    /// <summary>
    /// Note: This status effect has no logic by itself. Instead the effect that adds
    /// pain does the checking.
    /// While this effect is active no new pain effect can be added.
    /// </summary>
    public class StatusEffectProtectionPain : ProtoStatusEffect
    {
        public override string Description =>
            "You are currently under the effect of strong painkillers. You don't feel any pain, whether you like it or not.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Pain protection";
    }
}