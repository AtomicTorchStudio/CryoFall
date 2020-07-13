namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming;

    public class TechGroupFarmingT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.FarmingDescription;

        public override string Name => TechGroupsLocalization.FarmingName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupFarmingT2>(completion: 1);
        }
    }
}