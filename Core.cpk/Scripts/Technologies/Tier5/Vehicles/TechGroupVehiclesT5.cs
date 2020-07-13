namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles;

    public class TechGroupVehiclesT5 : TechGroup
    {
        public override string Description => TechGroupsLocalization.VehiclesDescription;

        public override string Name => TechGroupsLocalization.VehiclesName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupVehiclesT4>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT4>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT4>(completion: 1);
            requirements.AddGroup<TechGroupChemistryT4>(completion: 1);
        }
    }
}