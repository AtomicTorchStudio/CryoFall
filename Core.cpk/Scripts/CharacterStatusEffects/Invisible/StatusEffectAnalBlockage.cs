namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible
{
    using AtomicTorch.CBND.GameApi.Resources;

    /// <summary>
    /// Yes...
    /// We've been having as much laughs as you are now when we came up with this effect :)
    /// But we needed a way to prevent players from abusing Peredozin, so there you have it :)
    /// </summary>
    public class StatusEffectAnalBlockage : ProtoStatusEffect
    {
        public override string Description => string.Empty;

        // an icon is not necessary (since this is an invisible effect)
        public override ITextureResource Icon => TextureResource.NoTexture;

        public override double IntensityAutoDecreasePerSecondValue => 1.0 / 600.0; // total of 10 minutes for max time

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => this.ShortId;

        public override double VisibilityIntensityThreshold => double.MaxValue;
    }
}