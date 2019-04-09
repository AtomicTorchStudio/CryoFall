namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    public enum StatusEffectKind
    {
        /// <summary>
        /// Positive effect (displayed with the green background with saturation depending on intensity).
        /// </summary>
        Buff,

        /// <summary>
        /// Negative effect (displayed with the yellow/orange/red background depending on intensity).
        /// </summary>
        Debuff,

        /// <summary>
        /// Neutral effect or protective effect (displayed with white background)
        /// </summary>
        Neutral
    }
}