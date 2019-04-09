namespace AtomicTorch.CBND.CoreMod.Perks
{
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.GameApi.Resources;

    public class PerkIncreaseLandClaimLimitT3 : ProtoPerk
    {
        public override ITextureResource Icon { get; }
            = new TextureResource("Icons/IconConstructionSite");

        public override string Name => string.Format(PerkIncreaseLandClaimLimitT1.TitleFormat, "+1");
    }
}