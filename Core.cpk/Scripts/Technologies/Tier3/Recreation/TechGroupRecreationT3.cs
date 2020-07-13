namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming;

    public class TechGroupRecreationT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.RecreationDescription;

        public override string Name => TechGroupsLocalization.RecreationName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCookingT2>(completion: 1.0);
            requirements.AddGroup<TechGroupFarmingT2>(completion: 1.0);
        }
    }
}