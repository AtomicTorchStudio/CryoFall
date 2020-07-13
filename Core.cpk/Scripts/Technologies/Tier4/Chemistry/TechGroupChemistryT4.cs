namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;

    public class TechGroupChemistryT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ChemistryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ChemistryName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupChemistryT3>(completion: 1);
        }
    }
}