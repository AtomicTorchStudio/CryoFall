namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles;

    public class TechGroupVehiclesT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.VehiclesDescription;

        public override string Name => TechGroupsLocalization.VehiclesName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupVehiclesT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT3>(completion: 1);
            requirements.AddGroup<TechGroupChemistryT3>(completion: 1);
        }
    }
}