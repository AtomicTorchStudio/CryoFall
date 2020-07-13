namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Offense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense;

    public class TechGroupOffenseT5 : TechGroup
    {
        public override string Description => TechGroupsLocalization.OffenseDescription;

        public override string Name => TechGroupsLocalization.OffenseName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseT4>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT4>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT4>(completion: 0.2);
        }
    }
}