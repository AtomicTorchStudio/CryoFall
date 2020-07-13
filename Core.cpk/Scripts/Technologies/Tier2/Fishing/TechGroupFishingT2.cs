namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Fishing
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupFishingT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.FishingDescription;

        public override string Name => TechGroupsLocalization.FishingName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCookingT1>(completion: 1.0);
            requirements.AddGroup<TechGroupFarmingT1>(completion: 1.0);
            requirements.AddGroup<TechGroupIndustryT1>(completion: 0.6);
            requirements.AddGroup<TechGroupConstructionT1>(completion: 0.6);
        }
    }
}