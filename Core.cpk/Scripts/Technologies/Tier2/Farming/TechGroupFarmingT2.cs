namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupFarmingT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.FarmingDescription;

        public override string Name => TechGroupsLocalization.FarmingName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupFarmingT1>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT1>(completion: 0.6);
        }
    }
}