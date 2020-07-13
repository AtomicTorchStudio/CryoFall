namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming
{
    public class TechGroupFarmingT1 : TechGroup
    {
        public override string Description => TechGroupsLocalization.FarmingDescription;

        public override string Name => TechGroupsLocalization.FarmingName;

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}