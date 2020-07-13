namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;

    public class TechGroupChemistryT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ChemistryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ChemistryName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupChemistryT2>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT2>(completion: 0.6);
        }
    }
}