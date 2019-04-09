namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    /// <summary>
    /// This effect doesn't do anything by itself, but it is taken into account in other effects.
    /// It was designed as a way to prevent people from abusing death-respawn cycle to gain advantage, e.g. by raiding radtowns
    /// using constant respawning.
    /// 
    /// Note: This effect is taken into account inside radiation effect calculation.
    /// </summary>
    public class StatusEffectWeakened : ProtoStatusEffect
    {
        public override string Description =>
            "You feel rather weak. It's probably a good idea to avoid being exposed to harmful situations, such as radiation.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Weakened";
    }
}