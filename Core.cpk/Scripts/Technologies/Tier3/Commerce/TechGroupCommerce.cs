namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Commerce
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

    public class TechGroupCommerce : TechGroup
    {
        public override string Description =>
            "Working with currency and facilitating commerce. Allows for minting and recycling of coins as well as building automated trading stations.";

        public override string Name => "Commerce";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry2>(completion: 0.8);
            requirements.AddGroup<TechGroupConstruction2>(completion: 0.8);
        }
    }
}