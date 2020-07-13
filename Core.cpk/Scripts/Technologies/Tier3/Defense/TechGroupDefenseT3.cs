namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;

    public class TechGroupDefenseT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DefenseDescription;

        public override string Name => TechGroupsLocalization.DefenseName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDefenseT2>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT2>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT2>(completion: 0.2);
        }
    }
}