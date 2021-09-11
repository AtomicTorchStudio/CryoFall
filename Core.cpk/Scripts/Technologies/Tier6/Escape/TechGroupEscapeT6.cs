namespace AtomicTorch.CBND.CoreMod.Technologies.Tier6.Escape
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier5.ExoticWeapons;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles;

    public class TechGroupEscapeT6 : TechGroup
    {
        public override string Description => TechGroupsLocalization.EscapeDescription;

        public override string Name => TechGroupsLocalization.EscapeName;

        public override TechTier Tier => TechTier.Tier6;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstructionT5>(completion: 1.0);
            requirements.AddGroup<TechGroupElectricityT5>(completion: 1.0);
            requirements.AddGroup<TechGroupIndustryT4>(completion: 1.0);
            requirements.AddGroup<TechGroupChemistryT4>(completion: 1.0);
            requirements.AddGroup<TechGroupVehiclesT5>(completion: 1.0);
            requirements.AddGroup<TechGroupExoticWeaponsT5>(completion: 1.0);
        }
    }
}