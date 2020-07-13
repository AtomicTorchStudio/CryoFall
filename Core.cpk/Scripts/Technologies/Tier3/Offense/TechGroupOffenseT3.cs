namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense;

    public class TechGroupOffenseT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.OffenseDescription;

        public override string Name => TechGroupsLocalization.OffenseName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseT2>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT2>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT2>(completion: 0.2);
        }
    }
}