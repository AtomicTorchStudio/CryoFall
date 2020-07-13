namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;

    public class TechGroupVehiclesT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.VehiclesDescription;

        public override string Name => TechGroupsLocalization.VehiclesName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT2>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT2>(completion: 1);
            requirements.AddGroup<TechGroupChemistryT2>(completion: 1);
        }
    }
}