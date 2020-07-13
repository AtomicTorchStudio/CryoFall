namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Commerce
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;

    public class TechGroupCommerceT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CommerceDescription;

        public override string Name => TechGroupsLocalization.CommerceName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT2>(completion: 0.8);
            requirements.AddGroup<TechGroupConstructionT2>(completion: 0.8);
        }
    }
}