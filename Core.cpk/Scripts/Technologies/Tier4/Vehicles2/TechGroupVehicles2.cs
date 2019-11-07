namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles;

    public class TechGroupVehicles2 : TechGroup
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        public override string Description => "Advanced combat vehicles and transportation.";

        public override string Name => "Vehicles 2";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupVehicles>(completion: 1);
            requirements.AddGroup<TechGroupIndustry3>(completion: 1);
            requirements.AddGroup<TechGroupConstruction3>(completion: 1);
            requirements.AddGroup<TechGroupChemistry2>(completion: 1);
        }
    }
}