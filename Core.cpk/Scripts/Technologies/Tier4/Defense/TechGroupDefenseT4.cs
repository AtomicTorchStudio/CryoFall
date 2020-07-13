namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;

    public class TechGroupDefenseT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DefenseDescription;

        public override string Name => TechGroupsLocalization.DefenseName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDefenseT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT3>(completion: 0.2);
        }
    }
}