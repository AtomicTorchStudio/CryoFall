namespace AtomicTorch.CBND.CoreMod.Perks
{
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Resources;

    public class PerkIncreaseLandClaimLimitT1 : ProtoPerk
    {
        // {0} is a number
        public const string TitleFormat = "Maximum number of land claims: {0}";

        private const int DefaultNumberOfLandClaims = 1;

        public override ITextureResource Icon { get; }
            = new TextureResource("Icons/IconConstructionSite");

        public override string Name
            => string.Format(TitleFormat,
                             DefaultNumberOfLandClaims
                             + LandClaimSystemConstants.SharedLandClaimsNumberLimitIncrease);

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddValue(this, StatName.LandClaimsMaxNumber, DefaultNumberOfLandClaims);
        }
    }
}