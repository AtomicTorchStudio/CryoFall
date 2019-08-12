namespace AtomicTorch.CBND.CoreMod.Perks
{
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Resources;

    public class PerkIncreaseLandClaimLimitT1 : ProtoPerk
    {
        // {0} is a number
        public const string TitleFormat = "Maximum number of land claims: {0}";

        public override ITextureResource Icon { get; }
            = new TextureResource("Icons/IconConstructionSite");

        public override string Name => string.Format(TitleFormat, "1");

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddValue(this, StatName.LandClaimsMaxNumber, 1);
        }
    }
}