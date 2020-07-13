namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Defense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;

    public class TechGroupDefenseT5 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DefenseDescription;

        public override string Name => TechGroupsLocalization.DefenseName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDefenseT4>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT4>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT4>(completion: 0.2);
        }
    }
}