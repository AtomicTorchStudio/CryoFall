namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupElectricityT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ElectricityDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ElectricityName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstructionT1>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT1>(completion: 1);
        }
    }
}