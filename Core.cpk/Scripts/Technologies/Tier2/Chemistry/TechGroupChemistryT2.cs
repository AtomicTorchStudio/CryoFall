namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupChemistryT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ChemistryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ChemistryName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT1>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT1>(completion: 0.8);
        }
    }
}