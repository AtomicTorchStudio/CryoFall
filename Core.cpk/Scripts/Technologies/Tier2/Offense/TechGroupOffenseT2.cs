namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense;

    public class TechGroupOffenseT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.OffenseDescription;

        public override string Name => TechGroupsLocalization.OffenseName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseT1>(completion: 1.0);
            requirements.AddGroup<TechGroupIndustryT1>(completion: 0.6);
        }
    }
}