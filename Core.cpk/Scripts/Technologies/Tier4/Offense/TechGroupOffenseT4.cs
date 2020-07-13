namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense;

    public class TechGroupOffenseT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.OffenseDescription;

        public override string Name => TechGroupsLocalization.OffenseName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT3>(completion: 0.2);
        }
    }
}