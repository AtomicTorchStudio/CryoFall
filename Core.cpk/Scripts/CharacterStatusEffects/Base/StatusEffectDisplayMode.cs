namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using AtomicTorch.CBND.GameApi;

    [Flags]
    [NotPersistent]
    public enum StatusEffectDisplayMode : ushort
    {
        None = 0,

        IconShowIntensityPercent = 1 << 0,

        IconShowTimeRemains = 1 << 1,

        TooltipShowIntensityPercent = 1 << 2,

        TooltipShowTimeRemains = 1 << 3
    }
}