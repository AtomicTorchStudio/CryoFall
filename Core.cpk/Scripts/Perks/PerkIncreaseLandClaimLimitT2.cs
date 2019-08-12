namespace AtomicTorch.CBND.CoreMod.Perks
{
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Resources;

    public class PerkIncreaseLandClaimLimitT2 : ProtoPerk
    {
        public override ITextureResource Icon { get; }
            = new TextureResource("Icons/IconConstructionSite");

        public override string Name => string.Format(PerkIncreaseLandClaimLimitT1.TitleFormat, "+1");

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddValue(this, StatName.LandClaimsMaxNumber, 1);
        }
    }
}